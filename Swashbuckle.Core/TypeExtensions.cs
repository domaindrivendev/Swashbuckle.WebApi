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
                var genericArguments = type.GetGenericArguments()
                    .Select(t => t.FriendlyId())
                    .ToArray();

                var builder = new StringBuilder(type.Name);

                return builder
                    .Replace(String.Format("`{0}", genericArguments.Count()), String.Empty)
                    .Append(String.Format("[{0}]", String.Join(",", genericArguments).TrimEnd(',')))
                    .ToString();
            }

            return type.Name;
        }
    }
}