﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using OneScript.Compilation;
using OneScript.Compilation.Binding;
using OneScript.Execution;
using OneScript.Language;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace ScriptEngine.Compiler
{
    public class DefaultCompilerBackend : ICompilerBackend
    {
        private readonly StackMachineCodeGenerator _codeGen;

        public DefaultCompilerBackend(IErrorSink errorSink)
        {
            _codeGen = new StackMachineCodeGenerator(errorSink);
        }
        
        public IDependencyResolver DependencyResolver { get; set; }
        
        public bool GenerateDebugCode { get; set; }
        
        public bool GenerateCodeStat { get; set; }
        
        public SymbolTable Symbols { get; set; }
        
        public IExecutableModule Compile(ModuleNode parsedModule, Type classType)
        {
            _codeGen.ProduceExtraCode = GetCodeFlags();
            _codeGen.DependencyResolver = DependencyResolver;
            return _codeGen.CreateModule(parsedModule, parsedModule.Source, Symbols);
        }

        private CodeGenerationFlags GetCodeFlags()
        {
            CodeGenerationFlags cs = CodeGenerationFlags.Always;
            if (GenerateDebugCode)
                cs |= CodeGenerationFlags.DebugCode;

            if (GenerateCodeStat)
                cs |= CodeGenerationFlags.CodeStatistics;

            return cs;
        }
    }
}
