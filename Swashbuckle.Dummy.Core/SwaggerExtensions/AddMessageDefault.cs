using System.Collections.Generic;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Swashbuckle.Dummy.SwaggerExtensions
{
    public class AddMessageDefault : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, System.Type type)
        {
            schema.@default = new { title = "A message", content = "Some content" };
        }
    }
}
