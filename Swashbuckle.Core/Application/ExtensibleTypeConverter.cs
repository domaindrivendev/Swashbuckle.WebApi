using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    public class ExtensibleTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Extensible).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            foreach (var fieldInfo in value.GetType().GetFields())
            {
                var fieldValue = fieldInfo.GetValue(value);
                if (fieldValue == null && serializer.NullValueHandling == NullValueHandling.Ignore)
                    continue;

                if (fieldInfo.Name == "extensions")
                {
                    foreach (var entry in (IDictionary<string, object>)fieldValue)
                    {
                        writer.WritePropertyName(entry.Key);
                        serializer.Serialize(writer, entry.Value);
                    }
                }
                else
                {
                    writer.WritePropertyName(PropertyNameFor(fieldInfo));
                    serializer.Serialize(writer, fieldInfo.GetValue(value));
                }
            }

            writer.WriteEndObject();
        }

        private string PropertyNameFor(FieldInfo fieldInfo)
        {
            var jsonAttribute = fieldInfo.GetCustomAttribute<JsonPropertyAttribute>();
            return (jsonAttribute != null)
                ? jsonAttribute.PropertyName
                : fieldInfo.Name;
        }
    }
}