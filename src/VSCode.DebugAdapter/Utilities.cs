﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace VSCode.DebugAdapter
{
    static class Utilities
    {
        private static readonly Regex VARIABLE = new Regex(@"\{(\w+)\}");

        public static string ConcatArguments(IEnumerable<string> args)
        {
            if (args == null)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var stringArg in args)
            {
                sb.Append(' ');
                sb.Append('\"');
                sb.Append(stringArg);
                sb.Append('\"');
            }

            return sb.ToString();
        }

        public static string ExpandVariables(string format, dynamic variables, bool underscoredOnly = true)
        {
            if (variables == null)
            {
                variables = new { };
            }
            Type type = variables.GetType();
            return VARIABLE.Replace(format, match => {
                string name = match.Groups[1].Value;
                if (!underscoredOnly || name.StartsWith("_"))
                {

                    PropertyInfo property = type.GetProperty(name);
                    if (property != null)
                    {
                        object value = property.GetValue(variables, null);
                        return value.ToString();
                    }
                    return '{' + name + ": not found}";
                }
                return match.Groups[0].Value;
            });
        }

        public static Encoding GetEncodingFromOptions(string optionsValue)
        {
            if (string.IsNullOrWhiteSpace(optionsValue))
            {
                throw new ArgumentException("Encoding must be specified");
            }

            return Encoding.GetEncoding(optionsValue);
        }
    }
}
