using System;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Swashbuckle.WebAssets;

namespace Swashbuckle.Application
{
    public class SwaggerUiConfig
    {
        private readonly Dictionary<string, EmbeddedResourceDescriptor> _customWebAssets;
        private readonly Dictionary<string, string> _templateValues;

        public SwaggerUiConfig(IEnumerable<string> discoveryPaths)
        {
            _customWebAssets = new Dictionary<string, EmbeddedResourceDescriptor>();

            _templateValues = new Dictionary<string, string>
            {
                { "%(StylesheetIncludes)", "" },
                { "%(DiscoveryBaseUrl)", "''" },
                { "%(SupportHeaderParams)", "false" },
                { "%(SupportedSubmitMethods)", "'get','post','put','delete'" },
                { "%(CustomScripts)", "" },
                { "%(DocExpansion)", "'none'" },
                { "%(OAuth2Enabled)", "false" },
                { "%(OAuth2ClientId)", "null" },
                { "%(OAuth2Realm)", "null" },
                { "%(OAuth2AppName)", "null" }
            };

            var discoveryPathStrings = discoveryPaths.Select(path => "'" + path + "'");
            _templateValues["%(DiscoveryPaths)"] = String.Join(",", discoveryPathStrings);

            // Use Swashbuckle specific index.html
            CustomWebAsset("index.html", GetType().Assembly, "Swashbuckle.SwaggerExtensions.index.html");

            // Enable swagger-ui by default
            Enabled = true;
        }

        internal bool Enabled { get; private set; }

        public void Disable()
        {
            Enabled = false;
        }

        public SwaggerUiConfig InjectStylesheet(Assembly resourceAssembly, string resourceName)
        {
            var path = "ext/" + resourceName;
            var stringBuilder = new StringBuilder(_templateValues["%(StylesheetIncludes)"]);

            stringBuilder.AppendLine("<link href='" + path + "' media='screen' rel='stylesheet' type='text/css' />");
            _templateValues["%(StylesheetIncludes)"] = stringBuilder.ToString();

            _customWebAssets[path] = new EmbeddedResourceDescriptor(resourceAssembly, resourceName);
            return this;
        }

        public SwaggerUiConfig SupportHeaderParams()
        {
            _templateValues["%(SupportHeaderParams)"] = "true";
            return this;
        }

        public SwaggerUiConfig SupportedSubmitMethods(HttpMethod[] httpMethods)
        {
            var httpMethodsString = String.Join(",",
                httpMethods.Select(method => "'" + method.ToString().ToLower() + "'"));

            _templateValues["%(SupportedSubmitMethods)"] = httpMethodsString;
            return this;
        }

        public SwaggerUiConfig DocExpansion(DocExpansion docExpansion)
        {
            _templateValues["%(DocExpansion)"] = "'" + docExpansion.ToString().ToLower() + "'";
            return this;
        }

        public SwaggerUiConfig InjectJavaScript(Assembly resourceAssembly, string resourceName)
        {
            var path = "ext/" + resourceName;

            var stringBuilder = new StringBuilder(_templateValues["%(CustomScripts)"]);

            if (stringBuilder.Length > 0)
                stringBuilder.Append(",");

            stringBuilder.Append("'" + path + "'");

            _templateValues["%(CustomScripts)"] = stringBuilder.ToString();
            _customWebAssets[path] = new EmbeddedResourceDescriptor(resourceAssembly, resourceName);
            return this;
        }

        public SwaggerUiConfig CustomWebAsset(string path, Assembly resourceAssembly, string resourceName)
        {
            _customWebAssets[path] = new EmbeddedResourceDescriptor(resourceAssembly, resourceName);
            return this;
        }

        public SwaggerUiConfig EnableDiscoveryUrlSelector()
        {
            InjectJavaScript(GetType().Assembly, "Swashbuckle.SwaggerExtensions.discoveryUrlSelector.js");
            return this;
        }

        public SwaggerUiConfig EnableOAuth2Support(string clientId, string realm, string appName)
        {
            _templateValues["%(OAuth2Enabled)"] = "true";
            _templateValues["%(OAuth2ClientId)"] = "'" + clientId + "'";
            _templateValues["%(OAuth2Realm)"] = "'" + realm + "'";
            _templateValues["%(OAuth2AppName)"] = "'" + appName + "'";
            return this;
        }

        public EmbeddedWebAssetProviderSettings ToUiProviderSettings()
        {
            return new EmbeddedWebAssetProviderSettings(_customWebAssets, _templateValues);
        }
    }

    public enum DocExpansion
    {
        None,
        List,
        Full
    }
}