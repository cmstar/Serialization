using System;
using System.Reflection;
using System.Runtime.CompilerServices;
#if NET35
using cmstar.Serialization.Net35;
#else
using System.Collections.Concurrent;
#endif

namespace cmstar.Util
{
    public static class ReflectionUtils
    {
        private static readonly ConcurrentDictionary<Type, object> DefaultValues
            = new ConcurrentDictionary<Type, object>();

        public static bool IsNullable(Type t)
        {
            ArgAssert.NotNull(t, "t");
            return !t.IsValueType || IsNullableType(t);
        }

        public static bool IsNullableType(Type t)
        {
            ArgAssert.NotNull(t, "t");

            return (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static Type GetUnderlyingType(Type t)
        {
            return IsNullableType(t) ? Nullable.GetUnderlyingType(t) : t;
        }

        public static bool IsOrIsSubClassOf(Type thisType, Type targetType)
        {
            ArgAssert.NotNull(thisType, "thisType");
            ArgAssert.NotNull(targetType, "targetType");

            return thisType == targetType || thisType.IsSubclassOf(targetType);
        }

        public static Type[] GetGenericArguments(Type type, Type genericTypeDefinition)
        {
            ArgAssert.NotNull(type, "type");
            ArgAssert.NotNull(genericTypeDefinition, "genericTypeDefinition");

            if (!genericTypeDefinition.IsGenericTypeDefinition)
            {
                var msg = string.Format(
                    "The type {0} is not a generic type definition.",
                    genericTypeDefinition.Name);
                throw new ArgumentException(msg, "genericTypeDefinition");
            }

            if (genericTypeDefinition.IsInterface)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition)
                    return type.GetGenericArguments();

                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (!interfaceType.IsGenericType)
                        continue;

                    if (interfaceType.GetGenericTypeDefinition() != genericTypeDefinition)
                        continue;

                    return interfaceType.GetGenericArguments();
                }
            }
            else
            {
                var baseType = type;
                do
                {
                    if (!baseType.IsGenericType)
                        continue;

                    if (baseType.GetGenericTypeDefinition() != genericTypeDefinition)
                        continue;

                    return baseType.GetGenericArguments();

                } while ((baseType = baseType.BaseType) != null);
            }

            return null;
        }

        public static bool IsAnonymousType(Type type)
        {
            ArgAssert.NotNull(type, "type");

            if (!type.IsGenericType)
                return false;

            if (!Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false))
                return false;

            if ((type.Attributes & TypeAttributes.NotPublic) != TypeAttributes.NotPublic)
                return false;

            return type.Name.Contains("AnonymousType");
        }

        public static object GetDefaultValue(Type type)
        {
            if (!type.IsValueType)
                return null;

            object value;
            if (!DefaultValues.TryGetValue(type, out value))
            {
                value = Activator.CreateInstance(type);
                DefaultValues.TryAdd(type, value);
            }

            return value;
        }
    }
}
