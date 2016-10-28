﻿using System;
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
            var metadata = propInfo.DeclaringType.GetCustomAttributes(typeof(MetadataTypeAttribute), true).OfType<MetadataTypeAttribute>().ToArray().FirstOrDefault();
            if (metadata != null) {
                propInfo = metadata.MetadataClassType.GetProperties().SingleOrDefault(x => x.Name == propInfo.Name);
            }
            return propInfo != null && Attribute.IsDefined(propInfo, typeof (T));
        }

        public static PropertyInfo PropertyInfo(this JsonProperty jsonProperty)
        {
            if(jsonProperty.UnderlyingName == null) return null;
            return jsonProperty.DeclaringType.GetProperty(jsonProperty.UnderlyingName, jsonProperty.PropertyType);
        }
    }
}
