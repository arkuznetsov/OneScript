/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Text;

namespace ScriptEngine.Machine
{
    [Serializable]
    public struct AnnotationDefinition
    {
        public string Name;
        public AnnotationParameter[] Parameters;

        public int ParamCount => Parameters?.Length ?? 0;
    }
}
