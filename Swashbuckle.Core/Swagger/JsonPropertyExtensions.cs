using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.Swagger
{
    public static class JsonPropertyExtensions
    {
        public static PropertyInfo PropertyInfo(this JsonProperty jsonProperty)
        {
            return jsonProperty.DeclaringType.GetProperty(jsonProperty.UnderlyingName, jsonProperty.PropertyType);
        }

        public static bool IsRequired(this JsonProperty jsonProperty)
        {
            var propInfo = jsonProperty.PropertyInfo();

            return propInfo != null && Attribute.IsDefined(propInfo, typeof (RequiredAttribute));
        }
    }
}
