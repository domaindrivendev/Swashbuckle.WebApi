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
        private readonly Dictionary<string, string> _textReplacements;

        public SwaggerUiConfig(IEnumerable<string> discoveryPaths)
        {
            Enabled = true;

            _customWebAssets = new Dictionary<string, EmbeddedResourceDescriptor>();

            _textReplacements = new Dictionary<string, string>
            {
                { "%(StylesheetIncludes)", "" },
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
            _textReplacements["%(DiscoveryPaths)"] = String.Join(",", discoveryPathStrings);

            // Use Swashbuckle specific index.html
            CustomWebAsset("index.html", GetType().Assembly, "Swashbuckle.SwaggerExtensions.index.html");
        }

        internal bool Enabled { get; private set; }

        public void Disable()
        {
            Enabled = false;
        }

        public SwaggerUiConfig InjectStylesheet(Assembly resourceAssembly, string resourceName)
        {
            var path = "ext/" + resourceName;
            var stringBuilder = new StringBuilder(_textReplacements["%(StylesheetIncludes)"]);

            stringBuilder.AppendLine("<link href='" + path + "' media='screen' rel='stylesheet' type='text/css' />");
            _textReplacements["%(StylesheetIncludes)"] = stringBuilder.ToString();

            _customWebAssets[path] = new EmbeddedResourceDescriptor(resourceAssembly, resourceName);
            return this;
        }

        public SwaggerUiConfig SupportHeaderParams()
        {
            _textReplacements["%(SupportHeaderParams)"] = "true";
            return this;
        }

        public SwaggerUiConfig SupportedSubmitMethods(HttpMethod[] httpMethods)
        {
            var httpMethodsString = String.Join(",",
                httpMethods.Select(method => "'" + method.ToString().ToLower() + "'"));

            _textReplacements["%(SupportedSubmitMethods)"] = httpMethodsString;
            return this;
        }

        public SwaggerUiConfig DocExpansion(DocExpansion docExpansion)
        {
            _textReplacements["%(DocExpansion)"] = "'" + docExpansion.ToString().ToLower() + "'";
            return this;
        }

        public SwaggerUiConfig InjectJavaScript(Assembly resourceAssembly, string resourceName)
        {
            var path = "ext/" + resourceName;

            var stringBuilder = new StringBuilder(_textReplacements["%(CustomScripts)"]);

            if (stringBuilder.Length > 0)
                stringBuilder.Append(",");

            stringBuilder.Append("'" + path + "'");

            _textReplacements["%(CustomScripts)"] = stringBuilder.ToString();
            _customWebAssets[path] = new EmbeddedResourceDescriptor(resourceAssembly, resourceName);
            return this;
        }

        public SwaggerUiConfig CustomWebAsset(string path, Assembly resourceAssembly, string resourceName)
        {
            _customWebAssets[path] = new EmbeddedResourceDescriptor(resourceAssembly, resourceName);
            return this;
        }

        public SwaggerUiConfig EnableOAuth2Support(string clientId, string realm, string appName)
        {
            _textReplacements["%(OAuth2Enabled)"] = "true";
            _textReplacements["%(OAuth2ClientId)"] = "'" + clientId + "'";
            _textReplacements["%(OAuth2Realm)"] = "'" + realm + "'";
            _textReplacements["%(OAuth2AppName)"] = "'" + appName + "'";
            return this;
        }

        internal EmbeddedWebAssetProviderSettings ToUiProviderSettings()
        {
            return new EmbeddedWebAssetProviderSettings(_customWebAssets, _textReplacements);
        }
    }

    public enum DocExpansion
    {
        None,
        List,
        Full
    }
}