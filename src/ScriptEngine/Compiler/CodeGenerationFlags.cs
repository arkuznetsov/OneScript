/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace ScriptEngine.Compiler
{
    [Flags]
    public enum CodeGenerationFlags
    {
        Always = 0,
        CodeStatistics = 1,
        DebugCode = 2,
    }
}