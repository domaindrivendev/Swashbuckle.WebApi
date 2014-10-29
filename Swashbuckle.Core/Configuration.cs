using System;
using System.Net.Http;
using System.Web.Http;
using System.Linq;
using Swashbuckle.Application;

namespace Swashbuckle
{
    public class Configuration
    {
        public static Configuration Instance = new Configuration();

        private Func<HttpRequestMessage, string> _hostNameResolver;
        private Action<SwaggerDocsConfig> _configureDocs;
        private Action<SwaggerUiConfig> _configureUi;

        private Configuration()
        {
            _hostNameResolver = DefaultHostNameResolver();
        }

        public Configuration HostName(Func<HttpRequestMessage, string> hostNameResolver)
        {
            _hostNameResolver = hostNameResolver;
            return this;
        }

        public Configuration SwaggerDocs(Action<SwaggerDocsConfig> configure)
        {
            _configureDocs = configure;
            return this;
        }

        public Configuration SwaggerUi(Action<SwaggerUiConfig> configure)
        {
            _configureUi = configure;
            return this;
        }

        public void Init(HttpConfiguration httpConfig, string routePrefix = "swagger")
        {
            var docsConfig = new SwaggerDocsConfig(_hostNameResolver);
            _configureDocs(docsConfig);

            var discoveryPaths = docsConfig.VersionInfoBuilder.Build()
                .Select(entry => String.Format("/{0}/docs/{1}", routePrefix, entry.Key));

            var uiConfig = new SwaggerUiConfig(_hostNameResolver, discoveryPaths);
            _configureUi(uiConfig);

            if (uiConfig.Enabled)
            {
                httpConfig.Routes.MapHttpRoute(
                    "swagger_root",
                    routePrefix,
                    null,
                    null,
                    new RedirectHandler(_hostNameResolver, routePrefix + "/ui/index.html"));

                httpConfig.Routes.MapHttpRoute(
                    "swagger_ui",
                    routePrefix + "/ui/{*uiPath}",
                    null,
                    new { uiPath = @".+" },
                    new SwaggerUiHandler(uiConfig));
            }

            httpConfig.Routes.MapHttpRoute(
                "swagger_docs",
                routePrefix + "/docs/{apiVersion}",
                new { resourceName = RouteParameter.Optional },
                null,
                new SwaggerDocsHandler(docsConfig));
        }

        public static Func<HttpRequestMessage, string> DefaultHostNameResolver()
        {
            return (req) => req.RequestUri.Host + ":" + req.RequestUri.Port;
        }
    }
}