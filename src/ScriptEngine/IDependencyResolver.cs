﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Environment;

namespace ScriptEngine
{
    public interface IDependencyResolver
    {
        ExternalLibraryDef Resolve(ModuleInformation module, string libraryName);

        void Initialize(ScriptingEngine engine);
    }
}