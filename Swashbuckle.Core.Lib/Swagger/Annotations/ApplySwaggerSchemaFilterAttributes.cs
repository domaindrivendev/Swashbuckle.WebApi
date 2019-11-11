using System;
using System.Linq;
using System.Web.Http.Description;

namespace Swashbuckle.Swagger.Annotations
{
    public class ApplySwaggerSchemaFilterAttributes : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            var attributes = type.GetCustomAttributes(false).OfType<SwaggerSchemaFilterAttribute>();

            foreach (var attribute in attributes)
            {
                var filter = (ISchemaFilter)Activator.CreateInstance(attribute.FilterType);
                filter.Apply(schema, schemaRegistry, type);
            }
        }
    }
}
