using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Swashbuckle.Swagger.XmlComments
{
    public static class XmlCommentsExtensions
    {
        public static string XmlCommentsId(this Type type)
        {
            var builder = new StringBuilder(type.FullNameSansTypeParameters());
            builder.Replace("+", ".");

            if (type.IsGenericType)
            {
                var typeParameterQualifiers = type.GetGenericArguments()
                    .Select(t => t.XmlCommentsId())
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