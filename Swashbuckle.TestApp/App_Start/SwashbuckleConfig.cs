using System.Web.Http.Description;
using Swashbuckle.Area.Models;

namespace Swashbuckle.TestApp.App_Start
{
    public class SwashbuckleConfig
    {
        public static void Configure()
        {
            SwaggerGenerator.Configure()
                .AddOperationSpecFilter(new AddSupportedStatusCodesFilter());
        }
    }

    public class AddSupportedStatusCodesFilter : IOperationSpecFilter
    {
        public void UpdateSpec(ApiDescription apiDescription, ApiOperationSpec operationSpec)
        {
            operationSpec.errorResponses = new[]
                {
                    new ApiErrorResponseSpec {code = 200, reason = "OK"},
                    new ApiErrorResponseSpec {code = 400, reason = "Bad Request"},
                    new ApiErrorResponseSpec {code = 500, reason = "Internal Server Error"}
                };
        }
    }
}