/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

/*
 * Based on https://gist.github.com/NickStrupat/39e659e53a7aa000b737
 */

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OneScript.DebugProtocol.Internal
{
    internal static class AutoPropertyExtensions
    {
        private const string Prefix = "<";
        private const string Suffix = ">k__BackingField";

        private static string GetBackingFieldName(string propertyName) => $"{Prefix}{propertyName}{Suffix}";

        private static string GetBaseClassPrefix(Type baseClass) => baseClass.Name + "+";

        public static FieldInfo GetBackingField(this PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));
            if (!propertyInfo.CanRead || !propertyInfo.GetGetMethod(nonPublic: true)
                    .IsDefined(typeof(CompilerGeneratedAttribute), inherit: true))
                return null;

            var backingFieldName = GetBackingFieldName(propertyInfo.Name);
                        
            var backingField =
                propertyInfo.DeclaringType?.GetField(backingFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            
            if (backingField == null)
                return null;
            
            if (!backingField.IsDefined(typeof(CompilerGeneratedAttribute), inherit: true))
                return null;
            return backingField;
        }

        public static string GetFieldSerializationName(this FieldInfo field, PropertyInfo property)
        {
            if (field.DeclaringType == property.ReflectedType)
            {
                return field.Name;
            }
            
            // This is inherited prop
            return GetBaseClassPrefix(field.DeclaringType) + field.Name;
        }
    }
}