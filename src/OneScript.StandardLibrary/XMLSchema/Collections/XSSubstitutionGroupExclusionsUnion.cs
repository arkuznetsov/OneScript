﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Xml.Schema;
using OneScript.StandardLibrary.Collections;
using OneScript.StandardLibrary.XMLSchema.Enumerations;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.XMLSchema.Collections
{
    [ContextClass("ОбъединениеИсключенийГруппПодстановкиXS", "XSSubstitutionGroupExclusionsUnion")]
    public class XSSubstitutionGroupExclusionsUnion : AutoContext<XSSubstitutionGroupExclusionsUnion>
    {
        private readonly ArrayImpl _values;
        private bool Contains(XmlSchemaDerivationMethod _value)
        {
            XSSubstitutionGroupExclusions _enumValue = EnumerationXSSubstitutionGroupExclusions.FromNativeValue(_value);
            IValue _idx = _values.Find(_enumValue);
            return (_idx.DataType != DataType.Undefined);
        }

        internal XSSubstitutionGroupExclusionsUnion() => _values = ArrayImpl.Constructor();

        #region OneScript

        #region Properties

        [ContextProperty("Все", "All")]
        public bool All => Contains(XmlSchemaDerivationMethod.All);

        [ContextProperty("Ограничение", "Restriction")]
        public bool Restriction => Contains(XmlSchemaDerivationMethod.Restriction);

        [ContextProperty("Расширение", "Extension")]
        public bool Extension => Contains(XmlSchemaDerivationMethod.Extension);

        #endregion

        #region Methods

        [ContextMethod("Значения", "Values")]
        public ArrayImpl Values() => _values;

        #endregion

        #endregion
    }
}
