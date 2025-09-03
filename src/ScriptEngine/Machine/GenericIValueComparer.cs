/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Values;
using ScriptEngine.Machine.Contexts;
using OneScript.Execution;

namespace ScriptEngine.Machine
{
    public class GenericIValueComparer : IEqualityComparer<IValue>, IComparer<IValue>
    {
        private readonly IBslProcess _process;
        private readonly Func<IValue, IValue, int> _comparer;

        public GenericIValueComparer()
        {
            _comparer = CompareAsStrings;
        }

        public GenericIValueComparer(IBslProcess proc)
        {
            _process = proc;
            _comparer = CompareByPresentations;
        }

        public bool Equals(IValue x, IValue y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(IValue obj)
        {
            object CLR_obj;
            if (obj is BslUndefinedValue)
                return obj.GetHashCode();

            try
            {
                CLR_obj = ContextValuesMarshaller.ConvertToClrObject(obj);
            }
            catch (ValueMarshallingException)
            {
                CLR_obj = obj;
            }

            return CLR_obj.GetHashCode();
        }

        private int CompareAsStrings(IValue x, IValue y)
        {
            return x.ToString().CompareTo(y.ToString());
        }

        private int CompareByPresentations(IValue x, IValue y)
        {
            return ((BslValue)x).ToString(_process).CompareTo(((BslValue)y).ToString(_process));
        }

        public int Compare(IValue x, IValue y)
        {
           if (ReferenceEquals(x, y))
                return 0;
            
            if (x is IComparable && x.SystemType == y.SystemType )
                return x.CompareTo(y);
            else
                return _comparer(x,y);
        }
    }
}
