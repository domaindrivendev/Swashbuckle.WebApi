using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;

namespace Swashbuckle.Core.Models
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
            CustomScripts = new List<CustomResourceDescriptor>();
            CustomStylesheets = new List<CustomResourceDescriptor>();
        }

        public string ApiKeyName { get; set; }
        public string ApiKey { get; set; }
        public bool SupportHeaderParams { get; set; }
        public IEnumerable<HttpMethod> SupportedSubmitMethods { get; set; }
        public DocExpansion DocExpansion { get; set; }
        internal IList<CustomResourceDescriptor> CustomScripts { get; private set; }
        internal IList<CustomResourceDescriptor> CustomStylesheets { get; private set; }

        public static void Customize(Action<SwaggerUiConfig> customize)
        {
            customize(Instance);
        }

        public void AddOnCompleteScript(Assembly resourceAssembly, string resourceName)
        {
            CustomScripts.Add(new CustomResourceDescriptor(resourceAssembly, resourceName));
        }

        public void AddStylesheet(Assembly resourceAssembly, string resourceName)
        {
            CustomStylesheets.Add(new CustomResourceDescriptor(resourceAssembly, resourceName));
        }
    }

    internal class CustomResourceDescriptor
    {
        public CustomResourceDescriptor(Assembly assembly, string name)
        {
            Assembly = assembly;
            Name = name;
        }

        public Assembly Assembly { get; private set; }

        public string Name { get; private set; }
    }
}