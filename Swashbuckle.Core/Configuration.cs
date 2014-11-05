using System;
using System.Net.Http;
using System.Web.Http;
using System.Linq;
using Swashbuckle.Application;
using Swashbuckle.Swagger;
using Swashbuckle.WebAssets;

namespace Swashbuckle
{
    public class Configuration
    {
        public static Configuration Instance = new Configuration();

        private Func<HttpRequestMessage, string> _rootUrlResolver;
        private Action<SwaggerDocsConfig> _configureDocs;
        private Action<SwaggerUiConfig> _configureUi;

        private Configuration()
        {
            _rootUrlResolver = DefaultRootUrlResolver;
            _configureDocs = (c) => {}; 
            _configureUi = (c) => {}; 
        }

        public Configuration RootUrl(Func<HttpRequestMessage, string> rootUrlResolver)
        {
            _rootUrlResolver = rootUrlResolver;
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
            var swaggerDocsConfig = new SwaggerDocsConfig();
            _configureDocs(swaggerDocsConfig);

            var discoveryPaths = swaggerDocsConfig.VersionInfoBuilder.Build()
                .Select(entry => routePrefix + "/docs/" + entry.Key);
            var swaggerUiConfig = new SwaggerUiConfig(discoveryPaths);
            _configureUi(swaggerUiConfig);

            if (swaggerUiConfig.Enabled)
            {
                RegisterUiRoutes(httpConfig, routePrefix, swaggerUiConfig);
            }

            RegisterDocsRoutes(httpConfig, routePrefix, swaggerDocsConfig);
        }

        private void RegisterDocsRoutes(HttpConfiguration httpConfig, string routePrefix, SwaggerDocsConfig swaggerDocsConfig)
        {
            var swaggerProvider = new SwaggerGenerator(
                httpConfig.Services.GetApiExplorer(),
                httpConfig.GetJsonContractResolver(),
                swaggerDocsConfig.ToGeneratorSettings());

            httpConfig.Routes.MapHttpRoute(
                "swagger_docs",
                routePrefix + "/docs/{apiVersion}",
                new { resourceName = RouteParameter.Optional },
                null,
                new SwaggerDocsHandler(_rootUrlResolver, swaggerProvider));
        }

        private void RegisterUiRoutes(HttpConfiguration httpConfig, string routePrefix, SwaggerUiConfig swaggerUiConfig)
        {
            var swaggerUiProvider = new EmbeddedWebAssetProvider(swaggerUiConfig.ToUiProviderSettings());

            httpConfig.Routes.MapHttpRoute(
                "swagger_root",
                routePrefix,
                null,
                null,
                new RedirectHandler(_rootUrlResolver, routePrefix + "/ui/index.html"));

            httpConfig.Routes.MapHttpRoute(
                "swagger_ui",
                routePrefix + "/ui/{*uiPath}",
                null,
                new { uiPath = @".+" },
                new SwaggerUiHandler(_rootUrlResolver, swaggerUiProvider));
        }

        public static string DefaultRootUrlResolver(HttpRequestMessage request)
        {
            var virtualPathRoot = request.GetConfiguration().VirtualPathRoot;
            var requestUri = request.RequestUri;
            return String.Format("{0}://{1}:{2}{3}", requestUri.Scheme, requestUri.Host, requestUri.Port, virtualPathRoot);
        }
    }
}