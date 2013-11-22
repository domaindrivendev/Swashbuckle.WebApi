using System.Web.Http;

namespace Swashbuckle.Core
{
    public static class Bootstrapper
    {
        public static void Init(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                "swagger_default",
                "swagger",
                new { controller = "SwaggerUi", action = "Default" });

            config.Routes.MapHttpRoute(
                "swagger_ui",
                "swagger/ui/{*path}",
                new { controller = "SwaggerUi", action = "GetResource" });

            config.Routes.MapHttpRoute(
                "swagger_api_docs",
                "swagger/api-docs/{name}",
                new { controller = "SwaggerSpec", name = RouteParameter.Optional });
        }
    }
}
