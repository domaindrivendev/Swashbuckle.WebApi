using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;

namespace Swashbuckle.Application
{
    public class SwaggerUiConfig
    {
        internal static readonly SwaggerUiConfig StaticInstance = new SwaggerUiConfig();

        private SwaggerUiConfig()
        {
            SupportHeaderParams = false;
            SupportedSubmitMethods = new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put };
            DocExpansion = DocExpansion.None;
            CustomScripts = new List<InjectedResourceDescriptor>();
            CustomStylesheets = new List<InjectedResourceDescriptor>();
        }

        public bool SupportHeaderParams { get; set; }
        public IEnumerable<HttpMethod> SupportedSubmitMethods { get; set; }
        public DocExpansion DocExpansion { get; set; }
        internal IList<InjectedResourceDescriptor> CustomScripts { get; private set; }
        internal IList<InjectedResourceDescriptor> CustomStylesheets { get; private set; }

        public static void Customize(Action<SwaggerUiConfig> customize)
        {
            customize(StaticInstance);
        }

        public void InjectJavaScript(Assembly resourceAssembly, string resourceName)
        {
            CustomScripts.Add(new InjectedResourceDescriptor
            {
                RelativePath = String.Format("ext/{0}", resourceName),
                ResourceAssembly = resourceAssembly,
                ResourceName = resourceName,
            });
        }

        public void InjectStylesheet(Assembly resourceAssembly, string resourceName)
        {
            CustomStylesheets.Add(new InjectedResourceDescriptor
            {
                RelativePath = String.Format("ext/{0}", resourceName),
                ResourceAssembly = resourceAssembly,
                ResourceName = resourceName,
            });
        }
    }

    public enum DocExpansion
    {
        None,
        List,
        Full
    }

    internal class InjectedResourceDescriptor
    {
        public string RelativePath { get; set; }

        public Assembly ResourceAssembly { get; set; }

        public string ResourceName { get; set; }
    }
}