using System.Linq;
using System.Web.Http.Description;

namespace Swashbuckle.Swagger.Annotations
{
    public class ApplySwaggerOperationAttributes : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var attribute = apiDescription.ActionDescriptor.GetCustomAttributes<SwaggerOperationAttribute>()
                .FirstOrDefault();
            if (attribute == null) return;

            if (attribute.OperationId != null)
                operation.operationId = attribute.OperationId;

            if (attribute.Tags != null)
                operation.tags = attribute.Tags;

            if (attribute.Schemes != null)
                operation.schemes = attribute.Schemes;
        }
    }
}