﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler
{
    [Obsolete]
    public class ModuleCompilerContext : ICompilerContext
    {
        private readonly ICompilerContext _outerCtx;
        private readonly CompilerContext _moduleCtx;
        private int OUTER_CTX_SIZE;
        private int _localScopesCount = 0;

        public ModuleCompilerContext(ICompilerContext outerContext)
        {
            _outerCtx = outerContext;
            _moduleCtx = new CompilerContext();
            Update();
        }
        
        #region ICompilerContext Members

        public SymbolBinding DefineMethod(BslMethodInfo method)
        {
            var sb = _moduleCtx.DefineMethod(method);
            ShiftIndex(ref sb);

            return sb;
        }

        public SymbolBinding DefineProperty(string name, string alias = null)
        {
            var sb = _moduleCtx.DefineProperty(name, alias);
            ShiftIndex(ref sb);

            return sb;
        }

        public SymbolBinding DefineVariable(string name, string alias = null)
        {
            var sb = _moduleCtx.DefineVariable(name, alias);
            ShiftIndex(ref sb);

            return sb;
        }

        public SymbolBinding GetMethod(string name)
        {
            if(!_moduleCtx.TryGetMethod(name, out var sb))
                return _outerCtx.GetMethod(name);

            ShiftIndex(ref sb);
            return sb;
        }

        public bool TryGetMethod(string name, out SymbolBinding binding)
        {
            if (!_moduleCtx.TryGetMethod(name, out binding))
                return _outerCtx.TryGetMethod(name, out binding);

            ShiftIndex(ref binding);
            return true;
        }

        public SymbolScope GetScope(int scopeIndex)
        {
            if (scopeIndex < OUTER_CTX_SIZE)
            {
                return _outerCtx.GetScope(scopeIndex);
            }
            else
            {
                return _moduleCtx.GetScope(scopeIndex - OUTER_CTX_SIZE);
            }
        }

        public VariableBinding GetVariable(string name)
        {
            if (!_moduleCtx.TryGetVariable(name, out var vb))
                return _outerCtx.GetVariable(name);

            ShiftIndex(ref vb.binding);
            return vb;

        }

        public bool TryGetVariable(string name, out VariableBinding binding)
        {
            if (!_moduleCtx.TryGetVariable(name, out binding))
                return _outerCtx.TryGetVariable(name, out binding);

            ShiftIndex(ref binding.binding);
            return true;

        }

        public SymbolScope PopScope()
        {
            var scope = _moduleCtx.PopScope();
            _localScopesCount--;

            return scope;

        }

        public void PushScope(SymbolScope scope)
        {
            _moduleCtx.PushScope(scope);
            _localScopesCount++;
        }

        public int TopIndex()
        {
            if (_localScopesCount > 0)
            {
                return _moduleCtx.TopIndex() + OUTER_CTX_SIZE;
            }
            else
            {
                return _outerCtx.TopIndex();
            }
        }

        #endregion

        private void ShiftIndex(ref SymbolBinding symbolBinding)
        {
            symbolBinding.ScopeNumber += OUTER_CTX_SIZE;
        }

        public void Update()
        {
            OUTER_CTX_SIZE = _outerCtx.TopIndex() + 1;
        }
    }
}
