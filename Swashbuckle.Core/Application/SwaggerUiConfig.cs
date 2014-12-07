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
        private readonly Func<HttpRequestMessage, string> _rootUrlResolver;

        private readonly Dictionary<string, EmbeddedAssetDescriptor> _customAssets;
        private readonly Dictionary<string, string> _templateValues;

        public SwaggerUiConfig(Func<HttpRequestMessage, string> rootUrlResolver, IEnumerable<string> discoveryPaths)
        {
            _rootUrlResolver = rootUrlResolver;

            _customAssets = new Dictionary<string, EmbeddedAssetDescriptor>();
            _templateValues = new Dictionary<string, string>
            {
                { "%(StylesheetIncludes)", "" },
                { "%(RootUrl)", "" },
                { "%(DiscoveryPaths)", String.Join("|", discoveryPaths) },
                { "%(BooleanValues)", "true|false" },
                { "%(CustomScripts)", "" },
                { "%(DocExpansion)", "none" },
                { "%(OAuth2Enabled)", "false" },
                { "%(OAuth2ClientId)", "" },
                { "%(OAuth2Realm)", "" },
                { "%(OAuth2AppName)", "" },
            };

            // Use Swashbuckle specific index.html
            CustomAsset("index.html", GetType().Assembly, "Swashbuckle.SwaggerUi.Assets.index.html");
        }

        public void InjectStylesheet(Assembly resourceAssembly, string resourceName)
        {
            var path = "ext/" + resourceName;
            var stringBuilder = new StringBuilder(_templateValues["%(StylesheetIncludes)"]);

            stringBuilder.AppendLine("<link href='" + path + "' media='screen' rel='stylesheet' type='text/css' />");
            _templateValues["%(StylesheetIncludes)"] = stringBuilder.ToString();

            _customAssets[path] = new EmbeddedAssetDescriptor(resourceAssembly, resourceName);
        }
        
        public void BooleanValues(IEnumerable<string> values)
        {
            _templateValues["%(BooleanValues)"] = String.Join("|", values);
        }

        public void InjectJavaScript(Assembly resourceAssembly, string resourceName)
        {
            var stringBuilder = new StringBuilder(_templateValues["%(CustomScripts)"]);

            if (stringBuilder.Length > 0)
                stringBuilder.Append("|");

            var path = "ext/" + resourceName;
            stringBuilder.Append(path);

            _templateValues["%(CustomScripts)"] = stringBuilder.ToString();
            _customAssets[path] = new EmbeddedAssetDescriptor(resourceAssembly, resourceName);
        }

        public void DocExpansion(DocExpansion docExpansion)
        {
            _templateValues["%(DocExpansion)"] = docExpansion.ToString().ToLower();
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
            _templateValues["%(OAuth2Enabled)"] = "true";
            _templateValues["%(OAuth2ClientId)"] = clientId;
            _templateValues["%(OAuth2Realm)"] = realm;
            _templateValues["%(OAuth2AppName)"] = appName;
        }

        internal Func<HttpRequestMessage, string> GetRootUrlResolver()
        {
            return _rootUrlResolver;
        }

        internal EmbeddedSwaggerUiProviderSettings GetUiProviderSettings()
        {
            return new EmbeddedSwaggerUiProviderSettings(_customAssets, _templateValues);
        }
    }

    public enum DocExpansion
    {
        None,
        List,
        Full
    }
}