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
        internal static readonly SwaggerUiConfig Instance = new SwaggerUiConfig();

        private SwaggerUiConfig()
        {
            ApiKeyName = "special-key";
            ApiKey = "special-key";
            SupportHeaderParams = false;
            SupportedSubmitMethods = new[] {HttpMethod.Get, HttpMethod.Post, HttpMethod.Put};
            DocExpansion = DocExpansion.None;
            CustomScripts = new List<ResourceDescriptor>();
            CustomStylesheets = new List<ResourceDescriptor>();
        }

        public string ApiKeyName { get; set; }
        public string ApiKey { get; set; }
        public bool SupportHeaderParams { get; set; }
        public IEnumerable<HttpMethod> SupportedSubmitMethods { get; set; }
        public DocExpansion DocExpansion { get; set; }
        internal IList<ResourceDescriptor> CustomScripts { get; private set; }
        internal IList<ResourceDescriptor> CustomStylesheets { get; private set; }

        public static void Customize(Action<SwaggerUiConfig> customize)
        {
            customize(Instance);
        }

        public void AddOnCompleteScript(Assembly resourceAssembly, string resourceName)
        {
            CustomScripts.Add(new ResourceDescriptor
                {
                    Name = resourceName,
                    Assembly = resourceAssembly,
                });
        }

        public void AddStylesheet(Assembly resourceAssembly, string resourceName)
        {
            CustomStylesheets.Add(new ResourceDescriptor
            {
                Name = resourceName,
                Assembly = resourceAssembly,
            });
        }
    }

    internal class ResourceDescriptor
    {
        public string Name { get; set; }

        public Assembly Assembly { get; set; }
    }
}