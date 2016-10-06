using System;
using System.Collections.Generic;
using Swashbuckle.Swagger;
using Swashbuckle.Dummy.Controllers;

namespace Swashbuckle.Dummy.SwaggerExtensions
{
    public class RecursiveCallSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            schema.properties = new Dictionary<string, Schema>();
            schema.properties.Add("ExtraProperty", schemaRegistry.GetOrRegister(typeof(Product)));
        }
    }
}