using System;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.Swagger
{
    public static class SchemaExtensions
    {
        public static Schema WithValidationProperties(this Schema schema, JsonProperty jsonProperty)
        {
            var propInfo = jsonProperty.PropertyInfo();
            if (propInfo == null)
                return schema;

            foreach (var attribute in propInfo.GetCustomAttributes(false))
            {
                var regex = attribute as RegularExpressionAttribute;
                if (regex != null)
                    schema.pattern = regex.Pattern;

                var range = attribute as RangeAttribute;
                if (range != null)
                {
                    schema.maximum = range.Maximum;
                    schema.minimum = range.Minimum;
                }
            }

            return schema;
        }

        public static bool IsRequired(this JsonProperty jsonProperty)
        {
            var propInfo = jsonProperty.PropertyInfo();

            return propInfo != null && Attribute.IsDefined(propInfo, typeof (RequiredAttribute));
        }

        private static PropertyInfo PropertyInfo(this JsonProperty jsonProperty)
        {
            return jsonProperty.DeclaringType.GetProperty(jsonProperty.UnderlyingName);
        }
    }
}
