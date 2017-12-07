using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Routing;
using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    public class SwaggerAreaEnabledConfiguration
    {
        private static readonly string DefaultRouteTemplate = "{area}/swagger/ui/{*assetPath}";

        private readonly HttpConfiguration _httpConfig;
        private readonly IDictionary<AreaDescription, AreaSwaggerConfigurationContext> _areasDiscoveryPaths;
        private readonly AreaSwaggerConfigurationContext _noAreaConfigurationContext;

        public SwaggerAreaEnabledConfiguration(
            HttpConfiguration httpConfig,
            IDictionary<AreaDescription, AreaSwaggerConfigurationContext> areasDiscoveryPaths,
            AreaSwaggerConfigurationContext noAreaConfigurationContext)
        {
            _httpConfig = httpConfig;
            _areasDiscoveryPaths = areasDiscoveryPaths;
            _noAreaConfigurationContext = noAreaConfigurationContext;
        }

        public void EnableSwaggerUi(Action<SwaggerUiConfig> configure = null)
        {
            EnableSwaggerUi(DefaultRouteTemplate, configure);
        }

        public void EnableSwaggerUi(
            string areaRouteTemplate,
            Action<SwaggerUiConfig> configure = null)
        {
            foreach (var area in _areasDiscoveryPaths.Keys)
            {
                var routeTemplate = areaRouteTemplate.Replace("{area}", area.Name.ToLowerInvariant());
                var configurationContext = _areasDiscoveryPaths[area];
                var config = new SwaggerUiConfig(configurationContext.DiscoveryPaths, configurationContext.RootUrlResolver);
                configure?.Invoke(config);

                _httpConfig.Routes.MapHttpRoute(
                    name: "swagger_ui" + routeTemplate,
                    routeTemplate: routeTemplate,
                    defaults: null,
                    constraints: new {assetPath = @".+"},
                    handler: new SwaggerUiHandler(config)
                );

                if (routeTemplate == DefaultRouteTemplate)
                {
                    _httpConfig.Routes.MapHttpRoute(
                        name: "swagger_ui_shortcut",
                        routeTemplate: "swagger",
                        defaults: null,
                        constraints: new
                        {
                            uriResolution = new HttpRouteDirectionConstraint(HttpRouteDirection.UriResolution)
                        },
                        handler: new RedirectHandler(configurationContext.RootUrlResolver, "swagger/ui/index"));
                }
            }

            var swaggerEnabledConfiguration = new SwaggerEnabledConfiguration(_httpConfig, _noAreaConfigurationContext.RootUrlResolver, _noAreaConfigurationContext.DiscoveryPaths);
            var noAreaRouteTemplate = areaRouteTemplate.Replace("{area}/", string.Empty);
            swaggerEnabledConfiguration.EnableSwaggerUi(noAreaRouteTemplate, configure);
        }
    }
}