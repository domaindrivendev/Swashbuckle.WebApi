using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Description;

namespace Swashbuckle.Dummy.SwaggerExtensions
{
    public class InternalServerErrorResponseOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            operation.responses["500"] = new Response { description = "Internal server error" };
        }
    }
}
