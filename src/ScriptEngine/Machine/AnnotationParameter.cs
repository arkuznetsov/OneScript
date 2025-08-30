/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;

namespace ScriptEngine.Machine
{
    [Serializable]
    public struct AnnotationParameter
    {
        public string Name;
        public int ValueIndex;

        [NonSerialized]
        public IValue RuntimeValue;
        
        public const int UNDEFINED_VALUE_INDEX = -1;

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return string.Format("[{0}]", ValueIndex);
            }
            if (ValueIndex == UNDEFINED_VALUE_INDEX && RuntimeValue == null)
            {
                return Name;
            }
            return string.Format("{0}=[{1}]", Name, RuntimeValue.ToString());
        }
    }
}
