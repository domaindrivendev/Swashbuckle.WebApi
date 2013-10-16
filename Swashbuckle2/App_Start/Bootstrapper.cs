using System.Web.Http;
using Swashbuckle.Handlers;
using Swashbuckle.Models;

namespace Swashbuckle.App_Start
{
    public static class Bootstrapper
    {
        public static void Init(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                "swagger-spec",
                "swagger/api-docs/{resource}",
                new { resource = RouteParameter.Optional },
                null,
                new SwaggerSpecMessageHandler());

            config.Routes.MapHttpRoute(
                "swagger-ui",
                "swagger/{*resource}",
                new { resource = RouteParameter.Optional },
                null,
                new SwaggerUiMessageHandler(SwaggerSpecConfig.Instance.BasePathResolver));
        }
    }
}
