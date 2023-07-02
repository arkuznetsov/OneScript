﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using OneScript.Contexts.Enums;
using OneScript.Types;

namespace ScriptEngine.Machine.Contexts
{
    public static class EnumContextHelper
    {
        public static (TypeDescriptor, TypeDescriptor) RegisterEnumType<TEnum, TValue>(ITypeManager typeManager) 
            where TEnum : EnumerationContext 
            where TValue : EnumerationValue
        {
            return RegisterEnumType(typeof(TEnum), typeof(TValue), typeManager);
        }
        
        public static (TypeDescriptor, TypeDescriptor) RegisterEnumType(Type enumClass, Type enumValueClass, ITypeManager typeManager)
        {
            var attribs = enumClass.GetCustomAttributes(typeof(SystemEnumAttribute), false);

            if (attribs.Length == 0)
                throw new InvalidOperationException($"Enum {enumClass} is not marked as SystemEnum");

            var enumMetadata = (SystemEnumAttribute)attribs[0];

            return RegisterEnumType(enumClass, enumValueClass, typeManager, enumMetadata);
        }

        public static (TypeDescriptor, TypeDescriptor) RegisterEnumType(
            Type enumClass,
            Type enumValueClass,
            ITypeManager typeManager,
            IEnumMetadataProvider enumMetadata)
        {
            var enumType = CreateEnumType(enumClass, enumMetadata);
            typeManager.RegisterType(enumType);

            var enumValueType = CreateEnumValueType(enumValueClass, enumMetadata);
            typeManager.RegisterType(enumValueType);
            
            return (enumType, enumValueType);
        }

        private static TypeDescriptor CreateEnumType(Type enumType, IEnumMetadataProvider metadata)
        {
            return new TypeDescriptor(
                metadata.TypeUUID == default ? Guid.NewGuid() : Guid.Parse(metadata.TypeUUID),
                "Перечисление" + metadata.Name,
                metadata.Alias != default? "Enum" + metadata.Alias : default,
                enumType
            );
        }

        private static TypeDescriptor CreateEnumValueType(Type enumValueClass, IEnumMetadataProvider metadata)
        {
            return new TypeDescriptor(
                metadata.ValueTypeUUID == default ? Guid.NewGuid() : Guid.Parse(metadata.ValueTypeUUID),
                metadata.Name,
                metadata.Alias,
                enumValueClass
            );
        }

        public static TOwner CreateClrEnumInstance<TOwner, TEnum>(ITypeManager typeManager, EnumCreationDelegate<TOwner> creator) 
            where TOwner : EnumerationContext
            where TEnum : struct
        {
            TOwner instance;

            TypeDescriptor enumType;
            TypeDescriptor enumValType;

            (enumType, enumValType) = EnumContextHelper.RegisterEnumType<TOwner, ClrEnumValueWrapper<TEnum>>(typeManager);

            instance = creator(enumType, enumValType);
            return instance;
        }
        
        public static ClrEnumValueWrapper<T> WrapClrValue<T>(
            this EnumerationContext owner,
            string name,
            string alias,
            T value)
            where T : struct
        {
            var wrappedValue = new ClrEnumValueWrapper<T>(owner, value); 
            owner.AddValue(name, alias, wrappedValue);
            return wrappedValue;
        }

    }

    public delegate T EnumCreationDelegate<T>(TypeDescriptor typeRepresentation, TypeDescriptor valuesType) where T : EnumerationContext;

}
