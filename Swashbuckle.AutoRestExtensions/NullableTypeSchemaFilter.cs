using System;
using Swashbuckle.Swagger;

namespace Swashbuckle.AutoRestExtensions
{
    public class NullableTypeSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            // Add nullable type information
            schema.vendorExtensions.Add("x-nullable", !type.IsValueType || Nullable.GetUnderlyingType(type) != null);
        }
    }
}
