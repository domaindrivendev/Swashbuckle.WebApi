using System;
using System.Linq;
using System.Net;
using System.Web.Http.Description;

namespace Swashbuckle.Swagger.Annotations
{
    public class AddResponseFromAttributes : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            // put the controller ones first
            var attributes =
                apiDescription.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<SwaggerResponseAttribute>()
                .Where(a => a.StatusCode > 0)
                .ToList();

            // then the action attributes so they can replace the controller ones
            var actionAttributes =
                apiDescription.ActionDescriptor.GetCustomAttributes<SwaggerResponseAttribute>()
                .Where(a => a.StatusCode > 0)
                .ToList();

            attributes.AddRange(actionAttributes);

            if (!attributes.Any())
            {
                return;
            }

            foreach (var attr in attributes)
            {
                var httpCode = attr.StatusCode.ToString();
                var description = attr.Description;
                if (description == null)
                {
                    // if we don't have a description, try to get it out of HttpStatusCode
                    HttpStatusCode val;
                    if (Enum.TryParse(attr.StatusCode.ToString(), true, out val))
                    {
                        description = val.ToString();
                    }
                }

                var response = new Response { description = description };
                if (attr.ResponseType != null)
                {
                    response = new Response
                    {
                        description = description,
                        schema = schemaRegistry.GetOrRegister(attr.ResponseType)
                    };
                }

                operation.responses[httpCode] = response;
            }
        }
    }
}