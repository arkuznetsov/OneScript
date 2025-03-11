/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using OneScript.DependencyInjection;
using OneScript.Execution;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Sources;
using OneScript.Values;
using ScriptEngine.Machine;

namespace OneScript.Compilation
{
    public abstract class CompilerFrontendBase : ICompilerFrontend
    {
        protected CompilerFrontendBase(
            PreprocessorHandlers handlers,
            IErrorSink errorSink,
            IServiceContainer services)
        {
            PreprocessorHandlers = handlers;
            ErrorSink = errorSink;
            Services = services;
        }
        
        /// <summary>
        /// Класс процесса для компиляции выражений и батчей.
        /// В них не может быть вызовов Использовать, а значит не может быть исполнен код в процессе компиляции
        /// </summary>
        protected class IllegalBslProcess : IBslProcess
        {
            public static IBslProcess Instance = new IllegalBslProcess();
            
            private IllegalBslProcess()
            {}
                
            public BslValue Run(BslObjectValue target, IExecutableModule module, BslScriptMethodInfo method, IValue[] arguments) 
                => throw new NotSupportedException();

            public IServiceContainer Services => throw new NotSupportedException();
        }

        public IErrorSink ErrorSink { get; }
        
        protected IServiceContainer Services { get; }

        private PreprocessorHandlers PreprocessorHandlers { get; }
        
        public bool GenerateDebugCode { get; set; }
        
        public bool GenerateCodeStat { get; set; }

        public IList<string> PreprocessorDefinitions { get; } = new List<string>();
        
        public SymbolTable SharedSymbols { get; set; }

        public SymbolScope FillSymbols(Type targetType)
        {
            var symbolsProvider = Services.Resolve<TypeSymbolsProviderFactory>();
            var typeSymbols = symbolsProvider.Get(targetType);
            ModuleSymbols = new SymbolScope();
            typeSymbols.FillSymbols(ModuleSymbols);

            return ModuleSymbols;
        }
        
        private SymbolScope ModuleSymbols { get; set; }
        
        public IExecutableModule Compile(SourceCode source, IBslProcess process, Type classType = null)
        {
            var lexer = CreatePreprocessor(source);
            var symbols = PrepareSymbols();
            var parsedModule = ParseSyntaxConstruction(lexer, source, p => p.ParseStatefulModule());

            return CompileInternal(symbols, parsedModule, classType, process);
        }

        public IExecutableModule CompileExpression(SourceCode source)
        {
            var lexer = new DefaultLexer
            {
                Iterator = source.CreateIterator()
            };
            var symbols = PrepareSymbols();
            var parsedModule = ParseSyntaxConstruction(lexer, source, p => p.ParseExpression());

            return CompileExpressionInternal(symbols, parsedModule);
        }

        public IExecutableModule CompileBatch(SourceCode source)
        {
            var lexer = CreatePreprocessor(source);
            var symbols = PrepareSymbols();
            var parsedModule = ParseSyntaxConstruction(lexer, source, p => p.ParseStatefulModule());

            return CompileBatchInternal(symbols, parsedModule);
        }

        protected abstract IExecutableModule CompileInternal(SymbolTable symbols, ModuleNode parsedModule, Type classType, IBslProcess process);
        
        protected abstract IExecutableModule CompileExpressionInternal(SymbolTable symbols, ModuleNode parsedModule);
        
        protected abstract IExecutableModule CompileBatchInternal(SymbolTable symbols, ModuleNode parsedModule);

        private SymbolTable PrepareSymbols()
        {
            var actualTable = new SymbolTable();
            if (SharedSymbols != default)
            {
                for (int i = 0; i < SharedSymbols.ScopeCount; i++)
                {
                    actualTable.PushScope(SharedSymbols.GetScope(i), SharedSymbols.GetBinding(i));
                }
            }

            ModuleSymbols ??= new SymbolScope();
            actualTable.PushScope(ModuleSymbols, null);

            return actualTable;
        }
        
        private ModuleNode ParseSyntaxConstruction(
            ILexer lexer,
            SourceCode source,
            Func<DefaultBslParser, BslSyntaxNode> action)
        {
            var parser = new DefaultBslParser(
                lexer,
                ErrorSink,
                PreprocessorHandlers);

            ModuleNode moduleNode;
            
            try
            {
                moduleNode = (ModuleNode) action(parser);
            }
            catch (SyntaxErrorException e)
            {
                e.ModuleName ??= source.Name;
                throw;
            }

            return moduleNode;
        }
        
        private ILexer CreatePreprocessor(
            SourceCode source)
        {
            var baseLexer = new DefaultLexer
            {
                Iterator = source.CreateIterator()
            };

            var conditionals = PreprocessorHandlers?.Get<ConditionalDirectiveHandler>();
            if (conditionals != default)
            {
                foreach (var constant in PreprocessorDefinitions)
                {
                    conditionals.Define(constant);
                }
            }

            var lexer = new PreprocessingLexer(baseLexer)
            {
                Handlers = PreprocessorHandlers,
                ErrorSink = ErrorSink
            };
            return lexer;
        }
    }
}
