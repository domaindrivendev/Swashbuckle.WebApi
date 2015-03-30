namespace Swashbuckle.Dummy.SwaggerExtensions
{
    using System.Linq;
    using System.Web.Http.Description;
    using Swashbuckle.Swagger;

    public class ErrorResponseOperationFilter : IOperationFilter
    {

        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {


            var customAttributesList =
                apiDescription.ActionDescriptor.GetCustomAttributes<SwaggerResponseModelAttribute>()
                    .Where(x => x.OperationId == operation.operationId)
                    .ToList();

            if (customAttributesList.Count == 0)
            {
                return;
            }

            foreach (var customAttributes in customAttributesList)
            {
                Schema schema = null;
                if (customAttributes.ModelType != null)
                {
                    schemaRegistry.GetOrRegister(customAttributes.ModelType);
                    schema = new Schema { @ref = "#/definitions/" + customAttributes.ModelType.Name };
                }
                operation.responses.Add(
                    customAttributes.StatusCode,
                    new Response { description = customAttributes.Description.Trim(), schema = schema });
            }
        }
    }
}
