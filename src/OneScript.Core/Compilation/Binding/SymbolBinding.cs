/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.Compilation.Binding
{
    [Serializable]
    public struct SymbolBinding : IEquatable<SymbolBinding>
    {
        public int ScopeNumber { get; set; }
        
        public int MemberNumber { get; set; }

        public bool Equals(SymbolBinding other)
        {
            return ScopeNumber == other.ScopeNumber && MemberNumber == other.MemberNumber;
        }
    }
}