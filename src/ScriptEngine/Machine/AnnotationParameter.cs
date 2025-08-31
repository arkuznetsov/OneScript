/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;

namespace ScriptEngine.Machine
{
    [Serializable]
    public struct AnnotationParameter
    {
        public string Name;

        [NonSerialized]
        public IValue RuntimeValue;
        
        public const int UNDEFINED_VALUE_INDEX = -1;

        
        public override string ToString()
        {
            var list = new List<string>();
            if (!string.IsNullOrEmpty(Name))
            {
                list.Add(Name);
            }
            if (RuntimeValue != null)
            {
                list.Add(RuntimeValue.ToString());
            }
            return string.Join("=", list);
        }
    }
}
