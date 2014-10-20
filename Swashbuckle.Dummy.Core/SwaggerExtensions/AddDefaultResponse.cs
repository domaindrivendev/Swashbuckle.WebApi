using System.Web.Http.Description;
using Swashbuckle.Swagger;
using System.Web.Http;

namespace Swashbuckle.Dummy.SwaggerExtensions
{
    public class AddDefaultResponse : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var errorSchema = schemaRegistry.FindOrRegister(typeof(HttpError));

            operation.responses.Add("default", new Response
                {
                    schema = errorSchema
                });
        }
    }
}
