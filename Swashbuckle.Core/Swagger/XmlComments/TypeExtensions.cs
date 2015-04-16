using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Swashbuckle.Swagger.XmlComments
{
    public static class XmlCommentsExtensions
    {
        public static string XmlLookupName(this Type type)
        {
            var builder = new StringBuilder(type.FullNameSansTypeParameters());
            return builder
                .Replace("+", ".")
                .ToString();
        }

        public static string XmlLookupNameWithTypeParameters(this Type type)
        {
            var builder = new StringBuilder(type.XmlLookupName());

            if (type.IsGenericType)
            {
                var typeParameterQualifiers = type.GetGenericArguments()
                    .Select(t => t.XmlLookupNameWithTypeParameters())
                    .ToArray();

                builder
                    .Replace(String.Format("`{0}", typeParameterQualifiers.Count()), String.Empty)
                    .Append(String.Format("{{{0}}}", String.Join(",", typeParameterQualifiers).TrimEnd(',')))
                    .ToString();
            }

            return builder.ToString();
        }
    }
}