/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.DebugProtocol;

namespace VSCode.DebugAdapter
{
    /// <summary>
    /// Провайдер переменных модуля (для будущего расширения)
    /// Требует расширения протокола отладки для получения переменных модуля
    /// </summary>
    public class ModuleScopeProvider : IVariablesProvider
    {
        private readonly int _threadId;
        private readonly int _frameIndex;
        // В будущем может потребоваться moduleId или другой идентификатор

        public ModuleScopeProvider(int threadId, int frameIndex)
        {
            _threadId = threadId;
            _frameIndex = frameIndex;
        }

        public Variable[] FetchVariables(IDebuggerService service)
        {
            // TODO: когда будет расширен протокол - заменить на GetModuleVariables или подобное
            // Пока возвращаем пустой массив
            throw new NotImplementedException("Module scope requires protocol extension");
        }

        public IVariablesProvider CreateChildProvider(int variableIndex)
        {
            throw new NotImplementedException("Module scope requires protocol extension");
        }
    }
}

