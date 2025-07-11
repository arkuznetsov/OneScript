/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OneScript.DebugProtocol;
using Serilog;
using VSCode.DebugAdapter.OscriptProtocols;
using VSCodeDebug;


namespace VSCode.DebugAdapter
{
    internal class OscriptDebugSession : DebugSession, IDebugEventListener
    {
        private DebugeeProcess _debuggee;
        private bool _startupPerformed = false;
        private readonly Handles<OneScript.DebugProtocol.StackFrame> _framesHandles;
        private readonly Handles<IVariableLocator> _variableHandles;

        private readonly ILogger Log = Serilog.Log.ForContext<OscriptDebugSession>(); 

        public OscriptDebugSession() : base(true, false)
        {
            _framesHandles = new Handles<OneScript.DebugProtocol.StackFrame>();
            _variableHandles = new Handles<IVariableLocator>();
        }
        
        private string AdapterID { get; set; }
        
        public override void Initialize(Response response, dynamic args)
        {
            LogCommandReceived();
            AdapterID = (string) args.adapterID;

            _debuggee = DebugeeFactory.CreateProcess(AdapterID, PathStrategy);

            SendResponse(response, new Capabilities
            {
                supportsConditionalBreakpoints = true,
                supportsFunctionBreakpoints = false,
                supportsConfigurationDoneRequest = true,
                supportsExceptionFilterOptions = true,
                exceptionBreakpointFilters = new dynamic[]
                {
                    new
                    {
                        filter = "uncaught",
                        label = "Необработанные исключения",
                        description = "Остановка при возникновении необработанного исключения",
                        supportsCondition = true,
                        conditionDescription = "Искомая подстрока текста исключения"
                    },
                    new
                    {
                        filter = "all",
                        label = "Все исключения",
                        description = "Остановка при возникновении любого исключения",
                        supportsCondition = true,
                        conditionDescription = "Искомая подстрока текста исключения"
                    }
                },
                supportsEvaluateForHovers = true,
                supportTerminateDebuggee = true
            });

            SendEvent(new InitializedEvent());
        }

        public override void Launch(Response response, dynamic args)
        {
            LogCommandReceived();
            try
            {
                Log.Debug("Initializing process settings");
                _debuggee.Init(args);
            }
            catch (InvalidDebugeeOptionsException e)
            {
                Log.Error(e, "Wrong options received {ErrorCode}: {Message}", e.ErrorCode, e.Message);
                SendErrorResponse(response, e.ErrorCode, e.Message);
                return;
            }

            SubscribeForDebuggeeProcessEvents();

            try
            {
                Log.Verbose("Starting debuggee");
                _debuggee.Start();
                Log.Information("Debuggee started");
            }
            catch (Exception e)
            {
                Log.Error(e, "Can't launch debuggee");
                SendErrorResponse(response, 3012, "Can't launch debugee ({reason}).", new { reason = e.Message });
                return;
            }
            
            try
            {
                var tcpConnector = new TcpDebugServerClient(_debuggee.DebugPort, this);
                tcpConnector.Connect();

                _debuggee.SetConnection(tcpConnector);
            }
            catch (Exception e)
            {
                _debuggee.Kill();
                SendEvent(new TerminatedEvent());
                Log.Error(e, "Can't connect to debug server");
                SendErrorResponse(response, 4550, "Can't connect: " + e.ToString());
                return;
            }

            SetupProtocol(args);
            
            SendResponse(response);
        }

        private void SubscribeForDebuggeeProcessEvents()
        {
            _debuggee.OutputReceived += (s, e) =>
            {
                Log.Debug("Output received {Output}", e.Content);
                SendOutput(e.Category, e.Content);
            };

            _debuggee.ProcessExited += (s, e) =>
            {
                Log.Information("Debuggee has exited");
                SendEvent(new TerminatedEvent());
            };
        }

        private void SetupProtocol(dynamic args)
        {
            int protocolVersion = GetFromContainer(args, "protocolVersion", 0);
            if (ProtocolVersions.IsValid(protocolVersion))
            {
                _debuggee.ProtocolVersion = protocolVersion;
            }
        }

        public override void Attach(Response response, dynamic arguments)
        {
            LogCommandReceived();
            SubscribeForDebuggeeProcessEvents();
            _debuggee.DebugPort = GetFromContainer(arguments, "debugPort", 2801);
            
            try
            {
                var tcpConnector = new TcpDebugServerClient(_debuggee.DebugPort, this);
                tcpConnector.Connect();
                Log.Debug("Connected to debuggee on port {Port}", _debuggee.DebugPort);
                
                _debuggee.SetConnection(tcpConnector);
                _debuggee.InitAttached();
            }
            catch (Exception e)
            {
                Log.Error(e, "Can't connect debuggee");
                SendErrorResponse(response, 4550, "Can't connect: " + e.ToString());
                return;
            }

            SetupProtocol(arguments);
            
            SendResponse(response);
        }

        public override void Disconnect(Response response, dynamic arguments)
        {
            LogCommandReceived(new { Terminate = arguments.terminateDebuggee });
            bool terminateDebuggee = arguments.terminateDebuggee == true;
            
            _debuggee.HandleDisconnect(terminateDebuggee);
            SendResponse(response);
        }

        public override void SetExceptionBreakpoints(Response response, dynamic arguments)
        {
            LogCommandReceived();
            Log.Verbose("Exception breakpoints: {@Data}", arguments);

            var acceptedFilters = new List<VSCodeDebug.Breakpoint>();
            var filters = new List<(string Id, string Condition)>();

            foreach(var filter in arguments.filters)
            {
                filters.Add((filter, ""));
                acceptedFilters.Add(new VSCodeDebug.Breakpoint(true));
            }

            foreach (var filterOption in arguments.filterOptions)
            {
                filters.Add((filterOption.filterId, filterOption.condition ?? ""));
                acceptedFilters.Add(new VSCodeDebug.Breakpoint(true));
            }

            _debuggee.SetExceptionsBreakpoints(filters.ToArray());

            SendResponse(response, new SetExceptionBreakpointsResponseBody(acceptedFilters));
        }

        public override void SetBreakpoints(Response response, dynamic arguments)
        {
            LogCommandReceived();
            Log.Verbose("Breakpoints: {@Data}", arguments);
            
            if ((bool)arguments.sourceModified)
            {
                if (_startupPerformed)
                {
                    SendErrorResponse(response, 1102, "Нельзя установить точку останова на модифицированный файл.");
                    return;
                }
                SendResponse(response, new SetBreakpointsResponseBody());
                return;
            }

            var path = (string) arguments.source.path;
            path = ConvertClientPathToDebugger(path);
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                // vscode иногда передает путь, где диск - маленькая буква
                path = NormalizeDriveLetter(path);
            }
            
            var breaks = new List<OneScript.DebugProtocol.Breakpoint>();

            foreach (var srcBreakpoint in arguments.breakpoints)
            {
                var bpt = new OneScript.DebugProtocol.Breakpoint
                {
                    Line = (int)srcBreakpoint.line,
                    Source = path,
                    Condition = srcBreakpoint.condition ?? string.Empty
                };
                breaks.Add(bpt);
            }

            if(breaks.Count == 0) // в целях сохранения интерфейса WCF придется сделать костыль на перех. период
            {
                var bpt = new OneScript.DebugProtocol.Breakpoint
                {
                    Line = 0,
                    Source = path
                };
                breaks.Add(bpt);
            }
            
            var confirmedBreaks = _debuggee.SetBreakpoints(breaks);
            var confirmedBreaksVSCode = new List<VSCodeDebug.Breakpoint>(confirmedBreaks.Length);
            for (int i = 0; i < confirmedBreaks.Length; i++)
            {
                confirmedBreaksVSCode.Add(new VSCodeDebug.Breakpoint(true, confirmedBreaks[i].Line));
            }
            
            SendResponse(response, new SetBreakpointsResponseBody(confirmedBreaksVSCode));  
        }

        private string NormalizeDriveLetter(string path)
        {
            if (Path.IsPathRooted(path))
                return path[0].ToString().ToUpperInvariant() + path.Substring(1);
            else
                return path;

        }

        public void ThreadStopped(int threadId, ThreadStopReason reason)
        {
            LogEventOccured();
            _framesHandles.Reset();
            _variableHandles.Reset();

            SendEvent(new StoppedEvent(threadId, reason.ToString()));
        }
        
        public void ThreadStoppedEx(int threadId, ThreadStopReason reason, string errorMessage)
        {
            LogEventOccured();
            _framesHandles.Reset();
            _variableHandles.Reset();

            if (!string.IsNullOrEmpty(errorMessage))
                SendOutput("stderr", errorMessage);

            SendEvent(new StoppedEvent(threadId, reason.ToString()));
        }
        
        public void ProcessExited(int exitCode)
        {
            LogEventOccured();
            SendEvent(new ExitedEvent(exitCode));
        }

        public override void ConfigurationDone(Response response, dynamic args)
        {
            if (_debuggee == null)
            {
                Log.Debug("Config Done. Process is not started");
                SendResponse(response);
                return;
            }
            Log.Debug("Config Done. Process is started, sending Execute");
            _debuggee.BeginExecution(-1);
            _startupPerformed = true;
            SendResponse(response);
        }
        
        public override void Continue(Response response, dynamic arguments)
        {
            LogCommandReceived();
            SendResponse(response);
            _debuggee.BeginExecution(-1);
        }

        public override void Next(Response response, dynamic arguments)
        {
            LogCommandReceived();
            SendResponse(response);
            lock (_debuggee)
            {
                if (!_debuggee.HasExited)
                {
                    _debuggee.Next((int)arguments.threadId);
                }
            }
            
        }

        public override void StepIn(Response response, dynamic arguments)
        {
            LogCommandReceived();
            SendResponse(response);
            lock (_debuggee)
            {
                if (!_debuggee.HasExited)
                {
                    _debuggee.StepIn((int)arguments.threadId);
                }
            }
        }

        public override void StepOut(Response response, dynamic arguments)
        {
            LogCommandReceived();
            SendResponse(response);
            lock (_debuggee)
            {
                if (!_debuggee.HasExited)
                {
                    _debuggee.StepOut((int)arguments.threadId);
                }
            }
        }

        public override void Pause(Response response, dynamic arguments)
        {
            LogCommandReceived();
            throw new NotImplementedException();
        }

        public override void StackTrace(Response response, dynamic arguments)
        {
            LogCommandReceived();
            var firstFrameIdx = (int?)arguments.startFrame ?? 0;
            var limit = (int?) arguments.levels ?? 0;
            var threadId = (int) arguments.threadId;
            var processFrames = _debuggee.GetStackTrace(threadId, firstFrameIdx, limit);
            var frames = new VSCodeDebug.StackFrame[processFrames.Length];
            for (int i = 0; i < processFrames.Length; i++)
            {
                frames[i] = new VSCodeDebug.StackFrame(
                    _framesHandles.Create(processFrames[i]),
                    processFrames[i].MethodName,
                    processFrames[i].GetSource(),
                    processFrames[i].LineNumber, 0);
            }

            SendResponse(response, new StackTraceResponseBody(frames));
        }

        public override void Scopes(Response response, dynamic arguments)
        {
            LogCommandReceived();
            int frameId = GetFromContainer(arguments, "frameId", 0);
            var frame = _framesHandles.Get(frameId, null);
            if (frame == null)
            {
                SendErrorResponse(response, 10001, "No active stackframe");
                return;
            }
            
            var frameVariablesHandle = _variableHandles.Create(frame);
            var localScope = new Scope("Локальные переменные", frameVariablesHandle);
            SendResponse(response, new ScopesResponseBody(new Scope[] {localScope}));
        }

        public override void Variables(Response response, dynamic arguments)
        {
            LogCommandReceived();
            int varsHandle = GetFromContainer(arguments, "variablesReference", 0);
            var variables = _variableHandles.Get(varsHandle, null);
            if (variables == null)
            {
                SendErrorResponse(response, 10001, "No active stackframe");
                return;
            }

            _debuggee.FillVariables(variables);

            var responseArray = new VSCodeDebug.Variable[variables.Count];

            for (int i = 0; i < responseArray.Length; i++)
            {
                var variable = variables[i];

                if (variable.IsStructured && variable.ChildrenHandleID == 0)
                {
                    variable.ChildrenHandleID = _variableHandles.Create(variables.CreateChildLocator(i));
                }

                responseArray[i] = new VSCodeDebug.Variable(
                    variable.Name,
                    variable.Presentation,
                    variable.TypeName,
                    variable.ChildrenHandleID);
            }

            SendResponse(response, new VariablesResponseBody(responseArray));
        }

        public override void Threads(Response response, dynamic arguments)
        {
            LogCommandReceived();
            var threads = new List<VSCodeDebug.Thread>();
            var processThreads = _debuggee.GetThreads();
            for (int i = 0; i < processThreads.Length; i++)
            {
                threads.Add(new VSCodeDebug.Thread(processThreads[i], $"Thread {processThreads[i]}"));
            }
            
            SendResponse(response, new ThreadsResponseBody(threads));
        }

        public override void Evaluate(Response response, dynamic arguments)
        {
            LogCommandReceived();
            // expression, frameId, context
            int frameId = GetFromContainer(arguments, "frameId", 0);
            var frame = _framesHandles.Get(frameId, null);
            if (frame == null)
            {
                SendErrorResponse(response, 10001, "No active stackframe");
                return;
            }

            var expression = (string) arguments.expression;
            var context = (string) arguments.context;
            
            Log.Debug("Evaluate {Expression} in {Context}", expression, context);
             
            int id = -1;
            OneScript.DebugProtocol.Variable evalResult;
            try
            {
                evalResult = _debuggee.Evaluate(frame, expression);

                if (evalResult.IsStructured)
                {
                    var loc = new EvaluatedVariableLocator(expression, frame.ThreadId, frame.Index);
                    id = _variableHandles.Create(loc);
                }
            }
            catch (Exception e)
            {
                evalResult = new OneScript.DebugProtocol.Variable() { Presentation = e.Message, Name = "$evalFault" };
            }

            if (evalResult.Name.Equals("$evalFault") && context.Equals("hover"))
            {
                evalResult.Presentation = $"err: {expression}";
            }

            var protResult = new EvaluateResponseBody(evalResult.Presentation, id) {type = evalResult.TypeName};
            SendResponse(response, protResult);
        }


        private void SendOutput(string category, string data)
        {
            if (!String.IsNullOrEmpty(data))
            {
                if (data[data.Length - 1] != '\n')
                {
                    data += '\n';
                }
                SendEvent(new OutputEvent(category, data));
            }
        }

        private static T GetFromContainer<T>(dynamic container, string propertyName, T defaultValue = default)
        {
            try
            {
                return (T)container[propertyName];
            }
            catch (Exception)
            {
                // ignore and return default value
            }
            return defaultValue;
        }

        protected override void OnRequestError(Exception e)
        {
            Log.Error(e, "Unhandled request processing error");
        }

        private void LogCommandReceived(dynamic args = null, [CallerMemberName] string commandName = "")
        {
            if (args == null)
                Log.Debug("Command received {Command}", commandName);
            else
                Log.Debug("Command received {Command}: {@Args}", commandName, args);
        }
        
        private void LogEventOccured([CallerMemberName] string eventName = "")
        {
            Log.Debug("Event occured {Event}", eventName);
        }
    }
}
