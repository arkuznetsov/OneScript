/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Commons;

namespace ScriptEngine.HostedScript
{
    public class EnvironmentVariableConfigProvider : IConfigProvider
    {
        private readonly string _variableName;

        public EnvironmentVariableConfigProvider(string variableName)
        {
            _variableName = variableName;
        }
        
        public Func<IDictionary<string, string>> GetProvider()
        {
            var varName = _variableName;
            return () =>
            {
                // Читаем переменную окружения динамически при каждом вызове
                var envValue = Environment.GetEnvironmentVariable(varName);
                if (string.IsNullOrEmpty(envValue))
                    return new Dictionary<string, string>();
                
                var paramList = new FormatParametersList(envValue);
                return paramList.ToDictionary();
            };
        }
    }
}
