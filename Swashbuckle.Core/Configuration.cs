using System;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Application;

namespace Swashbuckle
{
    public class Configuration
    {
        public static Configuration Instance = new Configuration();

        private Func<HttpRequestMessage, string> _hostNameResolver;
        private readonly SwaggerDocsConfig _swaggerDocsConfig;
        private readonly SwaggerUiConfig _swaggerUiConfig;

        private Configuration()
        {
            _hostNameResolver = (req) => req.RequestUri.Host + ":" + req.RequestUri.Port;
            _swaggerDocsConfig = new SwaggerDocsConfig();
            _swaggerUiConfig = new SwaggerUiConfig();
        }

        public Configuration HostName(Func<HttpRequestMessage, string> hostNameResolver)
        {
            _hostNameResolver = hostNameResolver;
            return this;
        }

        public Configuration SwaggerDocs(Action<SwaggerDocsConfig> configure)
        {
            configure(_swaggerDocsConfig);
            return this;
        }

        public Configuration SwaggerUi(Action<SwaggerUiConfig> configure)
        {
            configure(_swaggerUiConfig);
            return this;
        }

        public void Init(HttpConfiguration httpConfig)
        {
            httpConfig.SetSwaggerDocsConfig(_swaggerDocsConfig);
            httpConfig.SetSwaggerUiConfig(_swaggerUiConfig);
            
            //httpConfig.Routes.MapHttpRoute(
            //    "swagger_root",
            //    "swagger",
            //    null,
            //    null,
            //    new RedirectHandler("swagger/ui/index.html"));

            httpConfig.Routes.MapHttpRoute(
                "swagger_ui",
                "swagger/ui/{*uiPath}",
                null,
                new { uiPath = @".+" },
                new SwaggerUiHandler());

            // TODO: Use config to assign constraint on apiVersion
            httpConfig.Routes.MapHttpRoute(
                "swagger_docs",
                "swagger/docs/{apiVersion}",
                new { resourceName = RouteParameter.Optional },
                null,
                new SwaggerDocsHandler());
        }
    }
}