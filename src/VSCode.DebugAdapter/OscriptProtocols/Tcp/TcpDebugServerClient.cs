/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using OneScript.DebugProtocol;
using OneScript.DebugProtocol.Abstractions;
using OneScript.DebugProtocol.TcpServer;
using Serilog;
using VSCode.DebugAdapter.OscriptProtocols;

namespace VSCode.DebugAdapter
{
    public class TcpDebugServerClient : IDebuggerService
    {
        private readonly int _port;
        private readonly IDebugEventListener _eventBackChannel;
        private BinaryChannel _commandsChannel;
        private RpcProcessor _processor;
        
        private readonly ILogger Log = Serilog.Log.ForContext<TcpDebugServerClient>();
        
        public TcpDebugServerClient(int port, IDebugEventListener eventBackChannel)
        {
            _port = port;
            _eventBackChannel = eventBackChannel;
        }

        private int _protocolVersion = ProtocolVersions.SafestVersion;
        
        public void Connect()
        {
            var debuggerUri = GetDebuggerUri(_port); 
            
            var client = new TcpClient();
            TryConnect(client, debuggerUri);
            
            _commandsChannel = new BinaryChannel(client);
            
            Log.Debug("Connected to {Host}:{Port}", debuggerUri.Host, debuggerUri.Port);
            ReconcileDataFormat(client);

            RunEventsListener(_commandsChannel);
        }

        private void ReconcileDataFormat(TcpClient client)
        {
            Log.Verbose("Sending reconcile message");
            var stream = client.GetStream();
            stream.Write(FormatReconcileUtils.FORMAT_RECONCILE_MAGIC, 0, FormatReconcileUtils.FORMAT_RECONCILE_MAGIC.Length);

            var pollResult = client.Client
                .Poll(FormatReconcileUtils.FORMAT_RECONCILE_TIMEOUT.Milliseconds * 1000, SelectMode.SelectRead);
            
            if (pollResult)
            {
                Log.Verbose("Reconcile data available. Waiting for full data");
                var attempts = 0;
                const int WAIT_ATTEMPTS = 3;
                const int ATTEMPT_INTERVAL = 250;
                
                var requiredDataLength = FormatReconcileUtils.FORMAT_RECONCILE_RESPONSE_PREFIX.Length + sizeof(int);
                while (client.Client.Available < requiredDataLength && attempts < WAIT_ATTEMPTS)
                {
                    attempts++;
                    Log.Verbose("Attempt # {Attempt}", attempts);
                    Thread.Sleep(ATTEMPT_INTERVAL);
                }

                if (client.Client.Available >= requiredDataLength)
                {
                    Log.Verbose("We have data available. Reading reconcile response");
                    var dataBuffer = new byte[requiredDataLength];
                    using var binaryReader = new BinaryReader(stream, Encoding.ASCII, true);
                    binaryReader.Read(dataBuffer, 0, FormatReconcileUtils.FORMAT_RECONCILE_RESPONSE_PREFIX.Length);

                    if (!FormatReconcileUtils.CheckReconcilePrefix(dataBuffer))
                    {
                        Log.Verbose("Received data is not reconcile message");
                        SelectSafestFormat();
                        return;
                    }

                    var formatVersion = binaryReader.ReadInt32();
                    Log.Verbose("Received format version {FormatVersion}", formatVersion);
                    _protocolVersion = ProtocolVersions.Adjust(formatVersion);
                    Log.Verbose("Active protocol version {ProtocolVersion}", _protocolVersion);
                }
                else
                {
                    SelectSafestFormat();
                }
            }
            else
            {
                SelectSafestFormat();
            }
        }

        private void SelectSafestFormat()
        {
            Log.Verbose("Reconcilation failed, selecting safest format");
            _protocolVersion = ProtocolVersions.SafestVersion;
        }

        public void Disconnect()
        {
            _processor.Stop();
            _commandsChannel.Dispose();
        }

        private static Uri GetDebuggerUri(int port)
        {
            var builder = new UriBuilder();
            builder.Scheme = "net.tcp";
            builder.Port = port;
            builder.Host = "localhost";

            return builder.Uri;
        }

        private void TryConnect(TcpClient client, Uri debuggerUri)
        {
            const int limit = 3;
            // TODO: параметризовать ожидания и попытки
            for (int i = 0; i < limit; ++i)
            {
                try
                {
                    client.Connect(debuggerUri.Host, debuggerUri.Port);
                    break;
                }
                catch (SocketException)
                {
                    if (i == limit - 1)
                        throw;
                    
                    Log.Warning("Error. Retry connect {Attempt}", i);
                    Thread.Sleep(1500);
                }
            }
        }

        private void RunEventsListener(ICommunicationChannel channelToListen)
        {
            var server = new DefaultMessageServer<TcpProtocolDtoBase>(channelToListen);
            server.ServerThreadName = "dbg-client-event-listener";
            
            _processor = new RpcProcessor(server);
            _processor.AddChannel(
                nameof(IDebugEventListener),
                typeof(IDebugEventListener),
                _eventBackChannel);
            
            _processor.AddChannel(
                nameof(IDebuggerService),
                typeof(IDebuggerService),
                this);
            
            _processor.Start();
        }

        private void WriteCommand<T>(T data, [CallerMemberName] string command = "")
        {
            Log.Verbose("Sending {Command} to debuggee, param {@Parameter}", command, data); 
            var dto = RpcCall.Create(nameof(IDebuggerService), command, data);
            _commandsChannel.Write(dto);
            Log.Verbose("Successfully written: {Command}", command);

        }
        
        private void WriteCommand(object[] data, [CallerMemberName] string command = "")
        {
            Log.Verbose("Sending {Command} to debuggee, params {@Parameters}", command, data);
            var dto = RpcCall.Create(nameof(IDebuggerService), command, data);
            _commandsChannel.Write(dto);
            Log.Verbose("Successfully written: {Command}", command);
        }
        
        private T GetResponse<T>()
        {
            var rpcResult = _processor.GetResult();
            Log.Verbose("Response received {Result} = {@Value}", rpcResult.Id, rpcResult.ReturnValue);
            if (rpcResult.ReturnValue is RpcExceptionDto excDto)
            {
                Log.Verbose("RPC Exception received: {Description}", excDto.Description);
                throw new RpcOperationException(excDto);
            }
            
            return (T) rpcResult.ReturnValue;
        }
        
        public void Execute(int threadId)
        {
            WriteCommand(threadId);
        }

        public void SetMachineExceptionBreakpoints((string Id, string Condition)[] filters)
        {
            WriteCommand(filters);
        }

        public Breakpoint[] SetMachineBreakpoints(Breakpoint[] breaksToSet)
        {
            WriteCommand(breaksToSet);
            return GetResponse<Breakpoint[]>();
        }

        public StackFrame[] GetStackFrames(int threadId)
        {
            WriteCommand(threadId);
            return GetResponse<StackFrame[]>();
        }

        public Variable[] GetVariables(int threadId, int frameIndex, int[] path)
        {
            WriteCommand(new object[]
            {
                threadId,
                frameIndex,
                path
            });

            return GetResponse<Variable[]>();
        }

        public Variable[] GetEvaluatedVariables(string expression, int threadId, int frameIndex, int[] path)
        {
            WriteCommand(new object[]
            {
                expression,
                threadId,
                frameIndex,
                path
            });

            return GetResponse<Variable[]>();
        }

        public Variable Evaluate(int threadId, int contextFrame, string expression)
        {
            WriteCommand(new object[]
            {
                threadId,
                contextFrame,
                expression
            });

            return GetResponse<Variable>();
        }

        public void Next(int threadId)
        {
            WriteCommand(threadId);
        }

        public void StepIn(int threadId)
        {
            WriteCommand(threadId);
        }

        public void StepOut(int threadId)
        {
            WriteCommand(threadId);
        }

        public void Disconnect(bool terminate)
        {
            WriteCommand(terminate);
            Disconnect();
        }

        public int[] GetThreads()
        {
            WriteCommand(null);
            return GetResponse<int[]>();
        }

        public int GetProcessId()
        {
            WriteCommand(null);
            return GetResponse<int>();
        }

        public int GetProtocolVersion()
        {
            WriteCommand(null);
            try
            {
                return ProtocolVersions.Adjust(GetResponse<int>());
            }
            catch (RpcOperationException e)
            {
                Log.Information("Checking version returned error: {Err}", e.Message);
                return ProtocolVersions.SafestVersion;
            }
        }
    }
}