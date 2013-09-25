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
            OnCompleteScripts = new List<EmbeddedElementDescriptor>();
            EmbeddedStylesheets = new List<EmbeddedElementDescriptor>();
        }

        public string ApiKeyName { get; set; }
        public string ApiKey { get; set; }
        public bool SupportHeaderParams { get; set; }
        public IEnumerable<HttpMethod> SupportedSubmitMethods { get; set; }
        public DocExpansion DocExpansion { get; set; }
        internal IList<EmbeddedElementDescriptor> OnCompleteScripts { get; private set; }
        internal IList<EmbeddedElementDescriptor> EmbeddedStylesheets { get; private set; }

        public static void Customize(Action<SwaggerUiConfig> customize)
        {
            customize(Instance);
        }

        public void AddOnCompleteScript(Assembly resourceAssembly, string resourceName)
        {
            OnCompleteScripts.AddEmbeddedElement(resourceAssembly, resourceName);
        }

        public void AddStylesheet(Assembly resourceAssembly, string resourceName)
        {
            EmbeddedStylesheets.AddEmbeddedElement(resourceAssembly, resourceName);
        }
    }

    internal static class Extensions
    {
        public static void AddEmbeddedElement(this IList<EmbeddedElementDescriptor> targetCollection,
                                        Assembly resourceAssembly, string resourceName)
        {
            targetCollection.Add(new EmbeddedElementDescriptor
            {
                RelativePath = String.Format("ext/{0}", resourceName),
                ResourceAssembly = resourceAssembly,
                ResourceName = resourceName,
            });
        }
    }

    internal class EmbeddedElementDescriptor
    {
        public string RelativePath { get; set; }

        public Assembly ResourceAssembly { get; set; }

        public string ResourceName { get; set; }
    }
}