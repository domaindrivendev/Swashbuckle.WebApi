using System.Web.Http;
using Swashbuckle.Core.Application;

namespace Swashbuckle.Core
{
    public static class Bootstrapper
    {
        public static void Init(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                "swagger_root",
                "swagger",
                null,
                null,
                new RedirectHandler("swagger/ui/index.html"));

            config.Routes.MapHttpRoute(
                "swagger_api_docs",
                "swagger/api-docs/{declarationName}",
                new { declarationName = RouteParameter.Optional },
                null,
                new SwaggerSpecHandler());

            config.Routes.MapHttpRoute(
                "swagger_ui",
                "swagger/ui/{*uiPath}",
                null,
                new { uiPath = @".+" },
                new SwaggerUiHandler());
        }
    }
}