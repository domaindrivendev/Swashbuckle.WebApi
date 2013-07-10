using System.Net.Http;
using System.Web.Http.Description;
using Swashbuckle.Models;

namespace Swashbuckle.TestApp.App_Start
{
    public class SwaggerConfig
    {
        public static void Customize()
        {
            SwaggerGeneratorConfig.Customize(c => c.AddFilter<AddSupportedStatusCodesFilter>());

            SwaggerUiConfig.Customize(c =>
                {
                    c.SupportHeaderParams = true;
                    c.DocExpansion = DocExpansion.Full;
                    c.SupportedSubmitMethods = new[] {HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Head};
                    c.AddOnCompleteScript(typeof(SwaggerConfig).Assembly, "Swashbuckle.TestApp.swagger_ui.ext.onComplete.js");
                });
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