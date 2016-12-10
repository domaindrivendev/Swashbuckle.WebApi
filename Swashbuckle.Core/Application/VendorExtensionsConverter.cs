using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    public class VendorExtensionsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.GetInterface(nameof(ISwaggerObject)) != null;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var instance = Activator.CreateInstance(objectType);
            serializer.Populate(jObject.CreateReader(), instance);

            if (objectType.GetInterface(nameof(ISwaggerObject)) != null)
            {
                var swaggerObject = instance as ISwaggerObject;
                foreach (var property in jObject.Properties().Where(p => p.Name.StartsWith("x-")))
                {
                    swaggerObject.vendorExtensions.Add(property.Name, property.Value);
                }
                return swaggerObject;
            }

            return instance;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jsonContract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(value.GetType());

            writer.WriteStartObject();

            foreach (var jsonProp in jsonContract.Properties) 
            {
                var propValue = jsonProp.ValueProvider.GetValue(value);
                if (propValue == null && serializer.NullValueHandling == NullValueHandling.Ignore)
                    continue;

                if (jsonProp.PropertyName == "vendorExtensions")
                {
                    var vendorExtensions = (IDictionary<string, object>)propValue;
                    if (vendorExtensions.Any()) 
                    {
                        foreach (var entry in vendorExtensions)
                        {
                            writer.WritePropertyName(entry.Key);
                            serializer.Serialize(writer, entry.Value);
                        }
                    }
                }
                else
                {
                    writer.WritePropertyName(jsonProp.PropertyName);
                    serializer.Serialize(writer, propValue);
                }
            }

            writer.WriteEndObject();
        }
    }
}