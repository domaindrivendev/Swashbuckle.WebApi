using System.Web.Http;
using Swashbuckle.Application;
using System.Web.Http.Routing;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;

namespace Swashbuckle
{
    public static class Bootstrapper
    {
        public static void Init(HttpConfiguration config)
        {
            Init(config, null);
        }

        public static void Init(HttpConfiguration config, string customIndexName)
        {
            config.Routes.MapHttpRoute(
                "swagger_root",
                "swagger",
                null,
                null,
                new RedirectHandler("swagger/ui/index.html"));

            if (customIndexName == null)
            {
                config.Routes.MapHttpRoute(
                    "swagger_ui",
                    "swagger/ui/{*uiPath}",
                    null,
                    new { uiPath = @".+" },
                    new SwaggerUiHandler());
            }
            else
            {
                config.Routes.MapHttpRoute(
                    "swagger_ui",
                    "swagger/ui/{*uiPath}",
                    null,
                    new { uiPath = string.Format(@"^(?!{0}).+$", customIndexName.Replace(".", @"\.")) },
                    new SwaggerUiHandler());
            }

            config.Routes.MapHttpRoute(
                "swagger_versioned_api_docs",
                "swagger/{apiVersion}/api-docs/{resourceName}",
                new { resourceName = RouteParameter.Optional },
                null,
                new SwaggerSpecHandler());

            config.Routes.MapHttpRoute(
                "swagger_api_docs",
                "swagger/api-docs/{resourceName}",
                new { resourceName = RouteParameter.Optional },
                null,
                new SwaggerSpecHandler());
        }
    }
}