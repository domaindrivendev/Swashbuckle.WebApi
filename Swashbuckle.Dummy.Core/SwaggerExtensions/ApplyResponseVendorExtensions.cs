using Swashbuckle.Swagger;
using System.Collections.Generic;
using System.Web.Http.Description;

namespace Swashbuckle.Dummy.SwaggerExtensions
{
    public class ApplyResponseVendorExtensions : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation.operationId != "Products_GetAllByType") return;

            var response = operation.responses["200"];
            response.vendorExtensions = new Dictionary<string, object>();
            response.vendorExtensions.Add("x-foo", "bar");
        }
    }
}
