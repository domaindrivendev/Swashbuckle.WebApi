using System;
using System.Collections.Generic;
using Swashbuckle.Swagger;

namespace Swashbuckle.Dummy.SwaggerExtensions
{
    public class ApplyCamelCasing : IModelFilter
    {
        public void Apply(DataType model, DataTypeRegistry dataTypeRegistry, Type type)
        {
            var camelCasedProperties = new Dictionary<string, DataType>();
            foreach (var entry in model.Properties)
            {
                var key = entry.Key;
                camelCasedProperties[Char.ToLower(key[0]) + key.Substring(1)] = entry.Value;
            }

            model.Properties = camelCasedProperties;
        }
    }
}