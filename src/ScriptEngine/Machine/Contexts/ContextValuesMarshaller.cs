/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Linq;
using System.Reflection;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Types;
using OneScript.Values;

namespace ScriptEngine.Machine.Contexts
{
    public static class ContextValuesMarshaller
    {
        public static MethodInfo BslParameterConverter { get; private set; }
        public static MethodInfo BslParameterGenericConverter { get; private set; }
        public static MethodInfo BslReturnValueGenericConverter { get; private set; }

        static ContextValuesMarshaller()
        {
            BslParameterConverter = typeof(ContextValuesMarshaller).GetMethods()
                .First(x => x.Name == nameof(ConvertParam) && x.GetGenericArguments().Length == 0 && x.GetParameters().Length == 3);
            
            BslParameterGenericConverter = typeof(ContextValuesMarshaller).GetMethods()
                .First(x => x.Name == nameof(ConvertParam) && x.GetGenericArguments().Length == 1 && x.GetParameters().Length == 3);
            
            BslReturnValueGenericConverter = typeof(ContextValuesMarshaller).GetMethods()
                .First(x => x.Name == nameof(ConvertReturnValue) && x.GetGenericArguments().Length == 1);
        }
        
        public static T ConvertParam<T>(IValue value, T defaultValue = default)
        {
            return ConvertParam<T>(value, ForbiddenBslProcess.Instance, defaultValue);
        }
        
        public static T ConvertParam<T>(IValue value, IBslProcess process, T defaultValue = default)
        {
            object valueObj = ConvertParam(value, typeof(T), process);
            return valueObj != null ? (T)valueObj : defaultValue;
        }
        
        public static object ConvertParam(IValue value, Type type)
        {
            return ConvertParam(value, type, ForbiddenBslProcess.Instance);
        }

        public static object ConvertParam(IValue value, Type type, IBslProcess process)
        {
            try
            {
                return ConvertValueType(value, type, process);
            }
            catch (InvalidCastException)
            {
                throw RuntimeException.InvalidArgumentType();
            }
            catch (OverflowException)
            {
                throw RuntimeException.InvalidArgumentValue();
            }
        }

        /// <summary>
        /// Выполняет строгую конвертацию парамтера в запрошенный тип.
        /// Не выполняет приведение объектов к строке, в отличие от ConvertParam.
        /// Это значит, что нельзя скормить объект в C# параметр с типом string через конверсию в AsString.
        /// Выдает исключение о неверном типе парамтера.
        /// </summary>
        public static T ConvertValueStrict<T>(IValue value)
        {
            if (value == null || value.IsSkippedArgument())
            {
                return default;
            }

            if (value is T t)
                return t;
            
            try
            {
                var converted = ConvertToClrObject(value);
                return converted switch
                {
                    T casted => casted,
                    decimal _ => (T)Convert.ChangeType(converted, typeof(T)),
                    _ => throw RuntimeException.InvalidArgumentType()
                };
            }
            catch (InvalidCastException)
            {
                throw RuntimeException.InvalidArgumentType();
            }
            catch (ValueMarshallingException)
            {
                throw RuntimeException.InvalidArgumentType();
            }
        }

        private static object ConvertValueType(IValue value, Type type, IBslProcess process)
        {
            object valueObj;
            if (value == null || value.IsSkippedArgument())
            {
                return null;
            }

            if (Nullable.GetUnderlyingType(type) != null)
            {
                return ConvertValueType(value, Nullable.GetUnderlyingType(type), process);
            }

            if (type == typeof(IVariable))
            {
                return value;
            }

            if (value is IValueReference r)
            {
                // Если целевой тип не требовал именно переменную, то разыменовываем ее
                value = r.Value;
            }

            if (type == typeof(IValue) || type == typeof(BslValue))
            {
                valueObj = value;
            }
            else if (value.SystemType == BasicTypes.Undefined)
            {
                // Если тип параметра не IValue и не IVariable && Неопределено -> null
                valueObj = null;
            }
            else if (type == typeof(string))
            {
                valueObj = value.AsString(process);
            }
            else if (type == typeof(int))
            {
                valueObj = (int)value.AsNumber();
            }
            else if (type == typeof(sbyte))
            {
                valueObj = (sbyte)value.AsNumber();
            }
            else if (type == typeof(short))
            {
                valueObj = (short)value.AsNumber();
            }
            else if (type == typeof(ushort))
            {
                valueObj = (ushort)value.AsNumber();
            }
            else if (type == typeof(uint))
            {
                valueObj = (uint)value.AsNumber();
            }
            else if (type == typeof(byte))
            {
                valueObj = (byte)value.AsNumber();
            }
            else if (type == typeof(long))
            {
                valueObj = (long)value.AsNumber();
            }
            else if (type == typeof(ulong))
            {
                valueObj = (ulong)value.AsNumber();
            }
            else if (type == typeof(double))
            {
                valueObj = (double)value.AsNumber();
            }
            else if (type == typeof(decimal))
            {
                valueObj = value.AsNumber();
            }
            else if (type == typeof(DateTime))
            {
                valueObj = value.AsDate();
            }
            else if (type == typeof(bool))
            {
                valueObj = value.AsBoolean();
            }
            else if (typeof(IRuntimeContextInstance).IsAssignableFrom(type))
            {
                valueObj = value.AsObject();
            }
            else if (value is EnumerationValue && typeof(EnumerationValue).IsAssignableFrom(type))
            {
                valueObj = value;
            }
            else
            {
                if (value is IObjectWrapper wrapped)
                {
                    if (!type.IsInstanceOfType(wrapped.UnderlyingObject))
                        throw new InvalidCastException();
                }
                else if (!type.IsInstanceOfType(value))
                {
                    throw new InvalidCastException();
                }

                valueObj = CastToClrObject(value);
            }

            return valueObj;
        }

        private static IValue ConvertReturnValue(object objParam, Type type)
        {
            if (objParam == null)
                return ValueFactory.Create();

            switch (objParam)
            {
                case IValue v: return v;

                case string s: return ValueFactory.Create(s);
                case bool b: return ValueFactory.Create(b);
                case DateTime d: return ValueFactory.Create(d);

                case int n: return ValueFactory.Create(n);
                case uint n: return ValueFactory.Create(n);
                case long n: return ValueFactory.Create(n);
                case ulong n: return ValueFactory.Create(n);
                case byte n: return ValueFactory.Create(n);
                case sbyte n: return ValueFactory.Create(n);
                case short n: return ValueFactory.Create(n);
                case ushort n: return ValueFactory.Create(n);
                case decimal n: return ValueFactory.Create(n);
                case double n: return ValueFactory.Create((decimal)n);
            }

            if (type.IsEnum)
            {
                return ConvertEnum(objParam, type);
            }
            else if (typeof(IRuntimeContextInstance).IsAssignableFrom(type))
            {
                return (IValue)(IRuntimeContextInstance)objParam;
            }
            else if (typeof(IValue).IsAssignableFrom(type))
            {
                return (IValue)objParam;
            }
            else if (Nullable.GetUnderlyingType(type) != null)
            {
                return ConvertReturnValue(objParam, Nullable.GetUnderlyingType(type));
            }
            else
            {
                throw ValueMarshallingException.TypeNotSupported(type);
            }
        }

        private static IValue ConvertEnum(object objParam, Type type)
        {
            return SimpleEnumsMarshaller.ConvertEnum(objParam, type);
        }

        public static T ConvertWrappedEnum<T>(IValue enumeration, T defValue) where T : struct
        {
            if (enumeration == null)
                return defValue;

            if (enumeration is ClrEnumValueWrapper<T> wrapped)
            {
                return wrapped.UnderlyingValue;
            }

            throw RuntimeException.InvalidArgumentValue();
        }

        public static IValue ConvertDynamicValue(object param)
        {
            if (param == null)
                throw ValueMarshallingException.InvalidNullValue();

            return ConvertReturnValue(param, param.GetType());
        }

        public static IValue ConvertDynamicIndex(object param)
        {
            if (param == null)
                throw ValueMarshallingException.InvalidNullIndex();

            return ConvertReturnValue(param, param.GetType());
        }

        public static IValue ConvertReturnValue<TRet>(TRet param)
        {
            return ConvertReturnValue(param, typeof(TRet));
        }

        public static object ConvertToClrObject(IValue value)
		{
            if (value == null)
                return null;

            // TODO: Вероятно, можно просто заменить на ассерт, что это не IValueReference
            if (value is IValueReference r)
                return ConvertToClrObject(r.Value);
            
            return value switch
            {
                BslNumericValue num => (decimal)num,
                BslBooleanValue boolean => (bool)boolean,
                BslStringValue str => (string)str,
                BslDateValue date => (DateTime)date,
                BslUndefinedValue _ => null,
                BslNullValue _ => null,
                BslTypeValue type => type.SystemType.ImplementingClass,
                IObjectWrapper wrapper => wrapper.UnderlyingObject,
                BslObjectValue obj => obj,
                _ => throw ValueMarshallingException.NoConversionToCLR(value.GetType())
            };
        }

        private static object CastToClrObject(IValue val)
        {
            object objectRef;
            if (val is IObjectWrapper wrapper)
            {
                objectRef = wrapper.UnderlyingObject;
            }
            else
            {
                objectRef = ConvertToClrObject(val);
            }

            return objectRef;
        }
    }
}
