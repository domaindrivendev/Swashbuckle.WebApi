using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Swashbuckle
{
    public static class TypeExtensions
    {
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

        public static string FriendlyId(this Type type)
        {
            if (type.IsGenericType)
            {
                var genericArgumentIds = type.GetGenericArguments()
                    .Select(t => t.FriendlyId())
                    .ToArray();

                return new StringBuilder(type.Name)
                    .Replace(String.Format("`{0}", genericArgumentIds.Count()), String.Empty)
                    .Append(String.Format("[{0}]", String.Join(",", genericArgumentIds).TrimEnd(',')))
                    .ToString();
            }

            return type.Name;
        }
    }
}