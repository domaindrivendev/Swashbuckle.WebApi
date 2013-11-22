using System;
using System.Collections.Generic;
using System.Linq;

namespace Swashbuckle.Core
{
    public static class TypeExtensions
    {
        public static string ShortName(this Type type)
        {
            return type.Name.Split('.').Last();
        }

        public static bool IsNullable(this Type type, out Type nullableTypeArgument)
        {
            nullableTypeArgument = null;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>))
            {
                nullableTypeArgument = type.GetGenericArguments().Single();
                return true;
            }

            return false;
        }

        public static bool IsEnumerable(this Type type, out Type enumerableTypeArgument)
        {
            enumerableTypeArgument = null;
            var enumerableType = type.GetInterfaces()
                .Union(new[] {type})
                .FirstOrDefault(
                    intfc => intfc.IsGenericType && intfc.GetGenericTypeDefinition() == typeof (IEnumerable<>));

            if (enumerableType != null)
                enumerableTypeArgument = enumerableType.GetGenericArguments().First();

            return enumerableType != null;
        }
    }
}