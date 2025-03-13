/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Values;

namespace ScriptEngine.Machine
{
    public class StackMachineExecutor : IExecutorProvider
    {
        private MachineInstance _machine;

        public Type SupportedModuleType => typeof(StackRuntimeModule);
        
        public Invoker GetInvokeDelegate()
        {
            return Executor;
        }

        private BslValue Executor(IBslProcess process, BslObjectValue target, IExecutableModule module, BslScriptMethodInfo method, IValue[] arguments)
        {
            if (!(method is MachineMethodInfo scriptMethodInfo))
            {
                throw new InvalidOperationException();
            }
            
            if (!(target is IRunnable runnable))
            {
                throw new InvalidOperationException();
            }

            var mustNotifyExit = false;
            var debugger = process.Services.TryResolve<IDebugController>();
            if (_machine == null)
            {
                _machine = new MachineInstance();
                _machine.Setup(process);

                if (debugger != default)
                {
                    _machine.SetDebugMode(debugger.ThreadManager, debugger.BreakpointManager);
                    mustNotifyExit = true;
                }
            }

            try
            {
                if (mustNotifyExit)
                {
                    debugger.ThreadManager.ThreadStarted(process.VirtualThreadId, _machine);
                }
                
                return (BslValue)_machine.ExecuteMethod(runnable, scriptMethodInfo, arguments);
            }
            finally
            {
                if (mustNotifyExit)
                {
                    debugger.ThreadManager.ThreadExited(process.VirtualThreadId);
                }
            }
        }
    }
}