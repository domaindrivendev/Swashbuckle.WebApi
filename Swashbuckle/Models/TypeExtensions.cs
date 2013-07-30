using System;
using System.Linq;

namespace Swashbuckle.Models
{
    internal static class TypeExtensions
    {
        public static Type AsGenericType(this Type source, Type genericType)
        {
            return source.GetInterfaces()
                .Union(new[] {source})
                .SingleOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericType);
        }

        public static bool IsNullableType(this Type source, out Type innerType)
        {
            var isNullable = source.IsGenericType && source.GetGenericTypeDefinition() == typeof (Nullable<>);
            if (isNullable)
            {
                innerType = source.GetGenericArguments().Single();
                return true;
            }

            innerType = null;
            return false;
        }
    }
}