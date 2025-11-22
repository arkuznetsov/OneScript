/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;

namespace ScriptEngine.Machine
{
    public struct ModuleSymbolBinding : IEquatable<ModuleSymbolBinding>
    {
        public IAttachableContext Target { get; set; }
        
        public int MemberNumber { get; set; }

        public bool Equals(ModuleSymbolBinding other)
        {
            return ReferenceEquals(Target, other.Target) && MemberNumber == other.MemberNumber;
        }

        public override bool Equals(object obj)
        {
            return obj is ModuleSymbolBinding other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Target, MemberNumber);
        }
    }
}