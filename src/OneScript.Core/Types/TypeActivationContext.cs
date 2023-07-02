/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.DependencyInjection;

namespace OneScript.Types
{
    public struct TypeActivationContext
    {
        public string TypeName { get; set; }
        
        public ITypeManager TypeManager { get; set; }

        public IServiceContainer Services { get; set; }
    }
}