/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Language.SyntaxAnalysis
{
    public class ParserOptions
    {
        public IErrorSink ErrorSink { get; set; } = new ThrowingErrorSink();
        
        public PreprocessorHandlers PreprocessorHandlers { get; set; }
    }
}