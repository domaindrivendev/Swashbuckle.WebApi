using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    public static class HttpAreaConfigurationExtensions
    {
        private static readonly string DefaultRouteTemplate = "{areaName}/swagger/docs/{apiVersion}";

        public static SwaggerAreaEnabledConfiguration EnableSwaggerPerArea(
            this HttpConfiguration httpConfig,
            Assembly assembly,
            Action<SwaggerDocsConfig, AreaDescription> configure = null,
            string routeAreaTemplate = null)
        {
            IDictionary<AreaDescription, AreaSwaggerConfigurationContext> areasConfigs = DiscoverAreas(assembly, configure, routeAreaTemplate);
            var allAreas = areasConfigs.Keys.ToList();

            foreach (var kv in areasConfigs)
            {
                var config = kv.Value.Config;
                var routeTemplate = kv.Value.RouteTemplate;
                var area = kv.Key;

                httpConfig.Routes.MapHttpRoute(
                    name: "swagger_docs" + routeTemplate,
                    routeTemplate: routeTemplate,
                    defaults: null,
                    constraints: new { apiVersion = @".+" },
                    handler: new SwaggerDocsHandler(config, area, allAreas)
                );
            }
            var noAreaConfigurationContext = EnableSwaggerOutOfAreas(httpConfig, configure, routeAreaTemplate, allAreas);

            return new SwaggerAreaEnabledConfiguration(httpConfig, areasConfigs, noAreaConfigurationContext);
        }

        private static AreaSwaggerConfigurationContext EnableSwaggerOutOfAreas(HttpConfiguration httpConfig, Action<SwaggerDocsConfig, AreaDescription> configure, 
            string routeAreaTemplate, IList<AreaDescription> allAreas)
        {
            var config = new SwaggerDocsConfig();
            configure?.Invoke(config, AreaDescription.Empty);

            var routeTemplate = (routeAreaTemplate ?? DefaultRouteTemplate)
                .Replace("{areaName}/", string.Empty);

            httpConfig.Routes.MapHttpRoute(
                name: "swagger_docs" + routeTemplate,
                routeTemplate: routeTemplate,
                defaults: null,
                constraints: new {apiVersion = @".+"},
                handler: new SwaggerDocsHandler(config, null, allAreas)
            );

            var discoveryPaths = config.GetApiVersions().Select(version => routeTemplate.Replace("{apiVersion}", version));

            return new AreaSwaggerConfigurationContext(config, discoveryPaths, routeTemplate);
        }

        public static IDictionary<AreaDescription, AreaSwaggerConfigurationContext> DiscoverAreas(Assembly assembly, 
            Action<SwaggerDocsConfig, AreaDescription> configure = null, string routeAreaTemplate = null)
        {
            var areasDiscoveryPaths = new Dictionary<AreaDescription, AreaSwaggerConfigurationContext>();

            foreach (var type in assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(AreaRegistration))))
            {
                var config = new SwaggerDocsConfig();
                var areaRegistration = (AreaRegistration)Activator.CreateInstance(type);
                var area = new AreaDescription(areaRegistration.AreaName, type);

                configure?.Invoke(config, area);

                var routeTemplate = (routeAreaTemplate ?? DefaultRouteTemplate).Replace("{areaName}", areaRegistration.AreaName.ToLowerInvariant());
                var discoveryPaths = config.GetApiVersions().Select(version => routeTemplate.Replace("{apiVersion}", version));

                areasDiscoveryPaths.Add(area, new AreaSwaggerConfigurationContext(config, discoveryPaths, routeTemplate));
            }

            return areasDiscoveryPaths;
        }
    }
}
