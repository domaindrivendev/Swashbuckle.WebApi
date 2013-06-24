using System.Net.Http;
using System.Web;
using System.Web.Http.Description;
using System.Web.Routing;
using Swashbuckle.Extended.App_Start;
using Swashbuckle.WebApi.Handlers;
using Swashbuckle.WebApi.Models;
using WebActivator;

[assembly: PostApplicationStartMethod(typeof (Bootstrapper), "Init")]

namespace Swashbuckle.Extended.App_Start
{
    public class Bootstrapper
    {
        public static void Init()
        {
            RouteTable.Routes.Add(new Route(
                "swagger-ext/{*path}",
                new SwaggerUiExtensionRouteHandler()));

            SwashbuckleConfig.Customize()
                .Generator(gc =>
                    gc.Filters.Add(new AddSupportedStatusCodesFilter()))
                .SwaggerUi(uc =>
                    {
                        uc.SupportHeaderParams = true;
                        uc.SupportedSubmitMethods = new[] {HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Head};
                        uc.DocExpansion = DocExpansionMode.Full;
                        uc.OnCompleteScriptPath = "/swagger-ext/after-load.js";
                    });
        }
    }

    public class SwaggerUiExtensionRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new EmbeddedResourceHttpHandler(GetType().Assembly, r => PathToResourceName(r.Path));
        }

        private static string PathToResourceName(string path)
        {
            return "Swashbuckle.Extended" + path
                .Replace("swagger-ext", "swagger_ext")
                .Replace("/", ".");
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