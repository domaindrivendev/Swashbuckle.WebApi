using System;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Swashbuckle.SwaggerUi;

namespace Swashbuckle.Application
{
    public class SwaggerUiConfig
    {
        private readonly Dictionary<string, EmbeddedAssetDescriptor> _customAssets;
        private readonly Dictionary<string, string> _replacements;
        private readonly Func<HttpRequestMessage, string> _rootUrlResolver;

        public SwaggerUiConfig(IEnumerable<string> discoveryPaths, Func<HttpRequestMessage, string> rootUrlResolver)
        {
            _customAssets = new Dictionary<string, EmbeddedAssetDescriptor>();
            _replacements = new Dictionary<string, string>
            {
                { "%(StylesheetIncludes)", "" },
                { "%(DiscoveryPaths)", String.Join("|", discoveryPaths) },
                { "%(BooleanValues)", "true|false" },
                { "%(CustomScripts)", "" },
                { "%(DocExpansion)", "none" },
                { "%(OAuth2Enabled)", "false" },
                { "%(OAuth2ClientId)", "" },
                { "%(OAuth2Realm)", "" },
                { "%(OAuth2AppName)", "" },
            };
            _rootUrlResolver = rootUrlResolver;

            // Use Swashbuckle specific index.html
            CustomAsset("index.html", GetType().Assembly, "Swashbuckle.SwaggerUi.Assets.index.html");
        }

        public void InjectStylesheet(Assembly resourceAssembly, string resourceName)
        {
            var path = "ext/" + resourceName;
            var stringBuilder = new StringBuilder(_replacements["%(StylesheetIncludes)"]);

            stringBuilder.AppendLine("<link href='" + path + "' media='screen' rel='stylesheet' type='text/css' />");
            _replacements["%(StylesheetIncludes)"] = stringBuilder.ToString();

            _customAssets[path] = new EmbeddedAssetDescriptor(resourceAssembly, resourceName);
        }
        
        public void BooleanValues(IEnumerable<string> values)
        {
            _replacements["%(BooleanValues)"] = String.Join("|", values);
        }

        public void InjectJavaScript(Assembly resourceAssembly, string resourceName)
        {
            var stringBuilder = new StringBuilder(_replacements["%(CustomScripts)"]);

            if (stringBuilder.Length > 0)
                stringBuilder.Append("|");

            var path = "ext/" + resourceName;
            stringBuilder.Append(path);

            _replacements["%(CustomScripts)"] = stringBuilder.ToString();
            _customAssets[path] = new EmbeddedAssetDescriptor(resourceAssembly, resourceName);
        }

        public void DocExpansion(DocExpansion docExpansion)
        {
            _replacements["%(DocExpansion)"] = docExpansion.ToString().ToLower();
        }

        public void CustomAsset(string path, Assembly resourceAssembly, string resourceName)
        {
            _customAssets[path] = new EmbeddedAssetDescriptor(resourceAssembly, resourceName);
        }

        public void EnableDiscoveryUrlSelector()
        {
            InjectJavaScript(GetType().Assembly, "Swashbuckle.SwaggerUi.Assets.discoveryUrlSelector.js");
        }

        public void EnableOAuth2Support(string clientId, string realm, string appName)
        {
            _replacements["%(OAuth2Enabled)"] = "true";
            _replacements["%(OAuth2ClientId)"] = clientId;
            _replacements["%(OAuth2Realm)"] = realm;
            _replacements["%(OAuth2AppName)"] = appName;
        }

        internal IAssetProvider GetSwaggerUiProvider()
        {
            return new EmbeddedAssetProvider(_customAssets, _replacements);
        }

        internal string GetRootUrl(HttpRequestMessage swaggerRequest)
        {
            return _rootUrlResolver(swaggerRequest);
        }
    }

    public enum DocExpansion
    {
        None,
        List,
        Full
    }
}