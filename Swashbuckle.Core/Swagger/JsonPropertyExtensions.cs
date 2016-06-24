using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.Swagger
{
    public static class JsonPropertyExtensions
    {
        public static bool IsRequired(this JsonProperty jsonProperty)
        {
            return jsonProperty.HasAttribute<RequiredAttribute>();
        }

        public static bool IsObsolete(this JsonProperty jsonProperty)
        {
            return jsonProperty.HasAttribute<ObsoleteAttribute>();
        }

        public static bool HasAttribute<T>(this JsonProperty jsonProperty)
        {
            var propInfo = jsonProperty.PropertyInfo();
            return propInfo != null && Attribute.IsDefined(propInfo, typeof (T));
        }

        public static PropertyInfo PropertyInfo(this JsonProperty jsonProperty)
        {
            if(jsonProperty.UnderlyingName == null) return null;

            var metadata = jsonProperty.DeclaringType.GetCustomAttributes(typeof(MetadataTypeAttribute), true)
                .FirstOrDefault();

            var typeToReflect = (metadata != null)
                ? ((MetadataTypeAttribute)metadata).MetadataClassType
                : jsonProperty.DeclaringType;

            return typeToReflect.GetProperty(jsonProperty.UnderlyingName, jsonProperty.PropertyType);
        }
    }
}
