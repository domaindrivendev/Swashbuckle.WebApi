using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;

namespace Swashbuckle.Models
{
    public enum DocExpansion
    {
        None,
        List,
        Full
    }

    public class SwaggerUiConfig
    {
        public static readonly SwaggerUiConfig Instance = new SwaggerUiConfig();

        private SwaggerUiConfig()
        {
            ApiKeyName = "special-key";
            ApiKey = "special-key";
            SupportHeaderParams = false;
            SupportedSubmitMethods = new[] {HttpMethod.Get, HttpMethod.Post, HttpMethod.Put};
            DocExpansion = DocExpansion.None;
            CustomScripts = new List<InjectedResourceDescriptor>();
            CustomStylesheets = new List<InjectedResourceDescriptor>();
        }

        public string ApiKeyName { get; set; }
        public string ApiKey { get; set; }
        public bool SupportHeaderParams { get; set; }
        public IEnumerable<HttpMethod> SupportedSubmitMethods { get; set; }
        public DocExpansion DocExpansion { get; set; }
        public IList<InjectedResourceDescriptor> CustomScripts { get; private set; }
        public IList<InjectedResourceDescriptor> CustomStylesheets { get; private set; }

        public static void Customize(Action<SwaggerUiConfig> customize)
        {
            customize(Instance);
        }

        public void AddOnCompleteScript(Assembly resourceAssembly, string resourceName)
        {
            CustomScripts.Add(new InjectedResourceDescriptor
                {
                    RelativePath = String.Format("ext/{0}", resourceName),
                    ResourceAssembly = resourceAssembly,
                    ResourceName = resourceName,
                });
        }

        public void AddStylesheet(Assembly resourceAssembly, string resourceName)
        {
            CustomStylesheets.Add(new InjectedResourceDescriptor
            {
                RelativePath = String.Format("ext/{0}", resourceName),
                ResourceAssembly = resourceAssembly,
                ResourceName = resourceName,
            });
        }
    }

    public class InjectedResourceDescriptor
    {
        public string RelativePath { get; set; }

        public Assembly ResourceAssembly { get; set; }

        public string ResourceName { get; set; }
    }
}