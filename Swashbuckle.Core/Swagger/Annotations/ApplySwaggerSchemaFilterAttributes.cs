using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;

namespace Swashbuckle.Swagger.Annotations
{
    public class ApplySwaggerSchemaFilterAttributes : ISchemaFilter
    {
        private IEnumerable<SwaggerSchemaFilterAttribute> Attributes(Type type)
        {
            var attributes = type.GetCustomAttributes(true).OfType<SwaggerSchemaFilterAttribute>();
            if (!attributes.Any() && type.BaseType != null)
                return Attributes(type.BaseType);
            else
                return attributes;
        }

        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            foreach (var attribute in Attributes(type))
            {
                var filter = (ISchemaFilter)Activator.CreateInstance(attribute.FilterType);
                filter.Apply(schema, schemaRegistry, type);
            }
        }
    }
}
