/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Globalization;

namespace OneScript.Localization
{
    public class BilingualString
    {
        private static readonly CultureInfo RussianCulture;

        static BilingualString()
        {
            try
            {
                RussianCulture = CultureInfo.GetCultureInfoByIetfLanguageTag("ru");
            }
            catch (CultureNotFoundException)
            {
            }
        }

        public static implicit operator BilingualString(string source)
        {
            return new BilingualString(source);
        }
        
        public static implicit operator string(BilingualString str)
        {
            return str.ToString();
        }

        private readonly int _ruHash;
        private readonly int _enHash;
        
        public BilingualString(string ru, string en)
        {
            Russian = ru;
            English = en;
            
            _enHash = en.GetHashCode();
            _ruHash = ru.GetHashCode();
        }
        
        public BilingualString(string single)
        {
            Russian = single;
            English = string.Empty;
            
            _enHash = English.GetHashCode();
            _ruHash = Russian.GetHashCode();
        }

        public string Russian { get; }
        
        public string English { get; }

        public override string ToString()
        {
            return Localize(Russian, English);
        }

        public bool HasName(string name, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase)
        {
            var nameHash = name.GetHashCode();
            if (nameHash != _ruHash && nameHash != _enHash)
                return false;
            
            return string.Equals(Russian, name, comparison) || string.Equals(English, name, comparison);
        }

        public static bool UseRussianLocale => CultureInfo.CurrentCulture.Parent.Equals(RussianCulture);

        public static string Localize(string russian, string english)
        {
            var currentCulture = CultureInfo.CurrentCulture;
            if (!Equals(currentCulture.Parent, RussianCulture))
            {
                return string.IsNullOrEmpty(english) ? russian : english;
            }

            return russian;
        }
    }
}