using System;
using System.Collections.Generic;
using System.Linq;
using Swashbuckle.Swagger;

namespace Swashbuckle.AutoRestExtensions
{
    public class NonNullableAsRequiredSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (schema.properties != null)
            {
                // Find properties with non-nullable types
                var nonNullableProperties = schema.properties
                    .Where(x => x.Value.HasNullable() && !x.Value.IsNullable())
                    .Select(x => x.Key)
                    .ToArray();

                if (nonNullableProperties.Length > 0)
                {
                    // Mark them as required
                    if (schema.required == null)
                        schema.required = new List<string>();
                    foreach (var property in nonNullableProperties)
                    {
                        if (!schema.required.Contains(property))
                            schema.required.Add(property);
                    }
                }
            }
        }
    }
}
