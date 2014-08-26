using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Linq;

namespace Swashbuckle.Application
{
    public class SwaggerUiConfig
    {
        internal static readonly SwaggerUiConfig StaticInstance = new SwaggerUiConfig();

        public SwaggerUiConfig()
        {
            SupportHeaderParams = false;
            SupportedSubmitMethods = new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put };
            DocExpansion = DocExpansion.None;
            CustomEmbeddedResources = new Dictionary<string, EmbeddedResource>();

            // Use Swashbuckle specific index.html
            CustomRoute("index.html", GetType().Assembly, "Swashbuckle.SwaggerExtensions.index.html");
        }

        public bool SupportHeaderParams { get; set; }
        public IEnumerable<HttpMethod> SupportedSubmitMethods { get; set; }
        public DocExpansion DocExpansion { get; set; }
        internal IDictionary<string, EmbeddedResource> CustomEmbeddedResources { get; private set; }

        public static void Customize(Action<SwaggerUiConfig> customize)
        {
            customize(StaticInstance);
        }

        public void EnableDiscoveryUrlSelector()
        {
            InjectJavaScript(GetType().Assembly, "Swashbuckle.SwaggerExtensions.discoveryUrlSelector.js");
        }

        public void InjectJavaScript(Assembly resourceAssembly, string resourceName)
        {
            var uiPath = String.Format("ext/{0}", resourceName);
            CustomEmbeddedResources[uiPath] = new EmbeddedResource(
                resourceAssembly,
                resourceName,
                "text/javascript");
        }

        public void InjectStylesheet(Assembly resourceAssembly, string resourceName)
        {
            var uiPath = String.Format("ext/{0}", resourceName);
            CustomEmbeddedResources[uiPath] = new EmbeddedResource(
                resourceAssembly,
                resourceName,
                "text/css");
        }

        public void CustomRoute(string uiPath, Assembly resourceAssembly, string resourceName)
        {
            CustomEmbeddedResources[uiPath] = new EmbeddedResource(resourceAssembly, resourceName);
        }
    }

    public enum DocExpansion
    {
        None,
        List,
        Full
    }
}