/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Compilation.Binding;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Localization;
using OneScript.Sources;

namespace OneScript.Native.Compiler
{
    public abstract class ExpressionTreeGeneratorBase : BslSyntaxWalker
    {
        private IErrorSink _errors;
        private SourceCode _sourceCode;
        private SymbolTable _ctx;

        protected ExpressionTreeGeneratorBase()
        {
        }
        
        protected ExpressionTreeGeneratorBase(IErrorSink errors)
        {
            _errors = errors;
        }
        
        protected ExpressionTreeGeneratorBase(BslWalkerContext context)
        {
            InitContext(context);
        }
        
        protected void InitContext(BslWalkerContext context)
        {
            InitContext(context.Errors, context.Source, context.Symbols);
        }
        
        protected void InitContext(IErrorSink errors, SourceCode lineInfo, SymbolTable symbols)
        {
            _errors = errors;
            _sourceCode = lineInfo;
            _ctx = symbols;
        }
        
        protected IErrorSink Errors => _errors;

        protected SymbolTable Symbols => _ctx;
        
        protected virtual BslWalkerContext MakeContext()
        {
            return new BslWalkerContext
            {
                Symbols = _ctx,
                Errors = _errors,
                Source = _sourceCode
            };
        }

        protected virtual void AddError(BilingualString errorText, CodeRange location)
        {
            Errors.AddError(new CodeError
            {
                Description = errorText.ToString(),
                Position = ToCodePosition(location)
            });
        }
        
        protected void AddError(CodeError err)
        {
            Errors.AddError(err);
        }
        
        protected void AddError(CodeError err, CodeRange location)
        {
            err.Position = ToCodePosition(location);
            Errors.AddError(err);
        }
        
        protected ErrorPositionInfo ToCodePosition(CodeRange range)
        {
            var codeLine = range.LineNumber > 0 ? _sourceCode.GetCodeLine(range.LineNumber)?.Trim() : "no code line";
            return new ErrorPositionInfo
            {
                Code = codeLine,
                LineNumber = range.LineNumber,
                ColumnNumber = range.ColumnNumber,
                ModuleName = _sourceCode.Name
            };
        }
    }
}