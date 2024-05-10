﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;

namespace ScriptEngine.Machine
{
    struct Scope
    {
        public IVariable[] Variables;
        public BslMethodInfo[] Methods;
        public IAttachableContext Instance;
    }

}
