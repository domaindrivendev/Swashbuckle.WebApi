using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;

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
            CustomScriptPaths = new List<string>();
            CustomStylesheetPaths = new List<string>();
            CustomRoutes = new Dictionary<string, CustomResourceDescriptor>();

            InjectJavaScript(GetType().Assembly, "Swashbuckle.SwaggerExtensions.versionSelector.js");
        }

        public bool SupportHeaderParams { get; set; }
        public IEnumerable<HttpMethod> SupportedSubmitMethods { get; set; }
        public DocExpansion DocExpansion { get; set; }
        internal IList<string> CustomScriptPaths { get; private set; }
        internal IList<string> CustomStylesheetPaths { get; private set; }
        internal IDictionary<string, CustomResourceDescriptor> CustomRoutes { get; private set; }

        public static void Customize(Action<SwaggerUiConfig> customize)
        {
            customize(StaticInstance);
        }

        public void InjectJavaScript(Assembly resourceAssembly, string resourceName)
        {
            var uiPath = String.Format("ext/{0}", resourceName);
            CustomScriptPaths.Add(uiPath);
            CustomRoute(uiPath, resourceAssembly, resourceName);
        }

        public void InjectStylesheet(Assembly resourceAssembly, string resourceName)
        {
            var uiPath = String.Format("ext/{0}", resourceName);
            CustomStylesheetPaths.Add(uiPath);
            CustomRoute(uiPath, resourceAssembly, resourceName);
        }

        public void CustomRoute(string uiPath, Assembly resourceAssembly, string resourceName)
        {
            CustomRoutes[uiPath] = new CustomResourceDescriptor
            {
                UiPath = uiPath,
                ResourceAssembly = resourceAssembly,
                ResourceName = resourceName,
            };
        }
    }

    public enum DocExpansion
    {
        None,
        List,
        Full
    }

    internal class CustomResourceDescriptor
    {
        public string UiPath { get; set; }

        public Assembly ResourceAssembly { get; set; }

        public string ResourceName { get; set; }
    }
}