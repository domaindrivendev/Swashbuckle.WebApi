using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;

namespace Swashbuckle.Swagger.Annotations
{
    public class ApplySwaggerSchemaFilterAttributes : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
			// create list of base types
			Stack<Type> types = new Stack<Type>();
			types.Push(type);

			Type t;
			while ((t = types.Peek().BaseType) != null)
			{
				types.Push(t);
			}

			// walk attributes in order base-type -> child-type
			// GetCustomAttributes(inherit: true) walks types in opposite direction and simple reverse would not suffice if multiple attributes are applied
			while (types.Count > 0)
			{
				foreach (SwaggerSchemaFilterAttribute attribute in types.Pop().GetCustomAttributes(typeof(SwaggerSchemaFilterAttribute), false))
				{
					var filter = (ISchemaFilter)Activator.CreateInstance(attribute.FilterType);
					filter.Apply(schema, schemaRegistry, type);
				}
			}
        }
    }
}
