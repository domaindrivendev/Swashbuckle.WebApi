using System;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Application;

namespace Swashbuckle
{
    public class Configuration
    {
        public static Configuration Instance = new Configuration();

        private readonly SwaggerDocsConfig _swaggerDocsConfig;
        private readonly SwaggerUi20Config _swaggerUiConfig;

        private Configuration()
        {
            _swaggerDocsConfig = new SwaggerDocsConfig();
            _swaggerUiConfig = new SwaggerUi20Config();
        }

        public Configuration SwaggerDocs(Action<SwaggerDocsConfig> configure)
        {
            configure(_swaggerDocsConfig);
            return this;
        }

        public Configuration SwaggerUi(Action<SwaggerUi20Config> configure)
        {
            configure(_swaggerUiConfig);
            return this;
        }

        public void Init(HttpConfiguration httpConfig)
        {
            httpConfig.SetSwaggerDocsConfig(_swaggerDocsConfig);
            httpConfig.SetSwaggerUiConfig(_swaggerUiConfig);
            
            httpConfig.Routes.MapHttpRoute(
                "swagger_root",
                "swagger",
                null,
                null,
                new RedirectHandler("swagger/ui/index.html"));

            httpConfig.Routes.MapHttpRoute(
                "swagger_ui",
                "swagger/ui/{*uiPath}",
                null,
                new { uiPath = @".+" },
                new SwaggerUi20Handler());

            // TODO: Use config to assign constraint on apiVersion
            httpConfig.Routes.MapHttpRoute(
                "swagger_docs",
                "swagger/docs/{apiVersion}",
                new { resourceName = RouteParameter.Optional },
                null,
                new SwaggerSpecHandler());
        }
    }
}