/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis
{
    public class NextLineRecoveryStrategy : IErrorRecoveryStrategy
    {
        public Lexem Recover(ILexer lexer)
        {
            lexer.SkipTillLineEnd();
            return lexer.NextLexem();
        }
    }
}