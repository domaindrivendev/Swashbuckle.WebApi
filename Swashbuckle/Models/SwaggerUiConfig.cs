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
            OnCompleteScripts = new List<EmbeddedResourceDescriptor>();
        }

        public string ApiKeyName { get; set; }
        public string ApiKey { get; set; }
        public bool SupportHeaderParams { get; set; }
        public IEnumerable<HttpMethod> SupportedSubmitMethods { get; set; }
        public DocExpansion DocExpansion { get; set; }
        internal IList<EmbeddedResourceDescriptor> OnCompleteScripts { get; private set; }

        public static void Customize(Action<SwaggerUiConfig> customize)
        {
            customize(Instance);
        }

        public void AddOnCompleteScript(Assembly resourceAssembly, string resourceName)
        {
            var path = String.Format("/swagger/ui/ext/onComplete_{0}.js", OnCompleteScripts.Count + 1);
            OnCompleteScripts.Add(new EmbeddedResourceDescriptor {Path = path, ResourceAssembly = resourceAssembly, ResourceName = resourceName});
        }
    }

    internal class EmbeddedResourceDescriptor
    {
        public string Path { get; set; }

        public Assembly ResourceAssembly { get; set; }

        public string ResourceName { get; set; }
    }
}