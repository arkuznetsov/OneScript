/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections;
using System.Collections.Generic;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Types;
using OneScript.Values;

namespace ScriptEngine.Machine.Contexts
{
    public class EnumerationContext : PropertyNameIndexAccessor, ICollectionContext<IValue>
    {
        private readonly List<EnumerationValue> _values = new List<EnumerationValue>();

        readonly IndexedNamesCollection _nameIds = new IndexedNamesCollection();
        private readonly TypeDescriptor _valuesType;

        public EnumerationContext(TypeDescriptor typeRepresentation, TypeDescriptor valuesType) : base(typeRepresentation)
        {
            _valuesType = valuesType;
        }

        public void AddValue(EnumerationValue val)
        {
            System.Diagnostics.Debug.Assert(val != null);

            _nameIds.RegisterName(val.Name, val.Alias);
            _values.Add(val);
        }

        public TypeDescriptor ValuesType => _valuesType;

        public EnumerationValue this[string name] => GetPropValueInternal(GetPropertyNumber(name));

        public int IndexOf(EnumerationValue enumVal)
        {
            return _values.IndexOf(enumVal);
        }

        public override int GetPropCount()
        {
            return _values.Count;
        }

        public override int GetPropertyNumber(string name)
        {
            int id;
            if (_nameIds.TryGetIdOfName(name, out id))
                return id;
            else
                return base.GetPropertyNumber(name);
        }

        public override bool IsPropReadable(int propNum)
        {
            return true;
        }

        public override IValue GetPropValue(int propNum)
        {
            return GetPropValueInternal(propNum);
        }
        
        private EnumerationValue GetPropValueInternal(int propNum)
        {
            return _values[propNum];
        }

        public override string GetPropName(int propNum)
        {
            return _values[propNum].ToString();
        }
        
        public override BslPropertyInfo GetPropertyInfo(int propNum)
        {
            var enumValue = _values[propNum];

            var propertyBuilder = BslPropertyBuilder.Create()
                .SetNames(enumValue.Name, enumValue.Alias)
                .CanRead(true)
                .CanWrite(false)
                .SetDispatchingIndex(propNum);
                
            if (_valuesType != null)
                propertyBuilder.ReturnType(_valuesType.ImplementingClass);
                
            return propertyBuilder.Build();
        }

        protected IList<EnumerationValue> ValuesInternal => _values;

        #region ICollectionContext Members

        public int Count(IBslProcess process)
        {
            return _values.Count;
        }

        public IEnumerator<IValue> GetEnumerator()
        {
            foreach (var item in _values)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        #endregion
    }
}
