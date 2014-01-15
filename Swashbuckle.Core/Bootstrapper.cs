using System.Web.Http;
using Swashbuckle.Core.Handlers;

namespace Swashbuckle.Core
{
    public static class Bootstrapper
    {
        public static void Init(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                "swagger_api_docs",
                "swagger/api-docs/{resource}",
                new {resource = RouteParameter.Optional},
                null,
                new SwaggerSpecHandler());

            config.Routes.MapHttpRoute(
                "swagger_ui",
                "swagger/{*path}",
                null,
                null,
                new SwaggerUiHandler());

//            config.Routes.MapHttpRoute(
//                "swagger_default",
//                "swagger",
//                new { controller = "SwaggerUi", action = "Default" });
//
//            config.Routes.MapHttpRoute(
//                "swagger_ui",
//                "swagger/ui/{*path}",
//                new { controller = "SwaggerUi", action = "GetResource" });
//
//            config.Routes.MapHttpRoute(
//                "swagger_api_docs",
//                "swagger/api-docs/{name}",
//                new { controller = "SwaggerSpec", name = RouteParameter.Optional });
        }
    }
}
