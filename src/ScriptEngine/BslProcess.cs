/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Contexts;
using OneScript.DependencyInjection;
using OneScript.Execution;
using OneScript.Values;
using ScriptEngine.Machine;

namespace ScriptEngine
{
    internal class BslProcess : IBslProcess
    {
        private readonly ExecutionContext _context;
        private readonly IDictionary<Type, Invoker> _bslExecutorsByModule;

        public BslProcess(ExecutionContext context, IEnumerable<IExecutorProvider> executorProviders)
        {
            _context = context;
            _bslExecutorsByModule =
                executorProviders.ToDictionary(item => item.SupportedModuleType, item => item.GetInvokeDelegate());
        }

        public IServiceContainer Services => _context.Services;
            
        public BslValue Run(BslObjectValue target, IExecutableModule module, BslScriptMethodInfo method, IValue[] arguments)
        {
            return _bslExecutorsByModule[module.GetType()](target, module, method, arguments);
        }
    }
}