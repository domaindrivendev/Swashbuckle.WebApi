using System;
using Swashbuckle.Swagger;

namespace Swashbuckle.Dummy.SwaggerExtensions
{
    public class ApplySchemaVendorExtensions : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            schema.extensions.Add("x-schema", "bar");
        }
    }
}
