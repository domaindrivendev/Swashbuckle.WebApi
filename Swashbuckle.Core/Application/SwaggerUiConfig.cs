using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Swashbuckle.SwaggerUi;

namespace Swashbuckle.Application
{
    public class SwaggerUiConfig
    {
        private readonly Dictionary<string, EmbeddedAssetDescriptor> _pathToAssetMap;
        private readonly Dictionary<string, string> _templateParams;
        private readonly Func<HttpRequestMessage, string> _rootUrlResolver;

        public SwaggerUiConfig(IEnumerable<string> discoveryPaths, Func<HttpRequestMessage, string> rootUrlResolver)
        {
            _pathToAssetMap = new Dictionary<string, EmbeddedAssetDescriptor>();

            _templateParams = new Dictionary<string, string>
            {
                { "%(DocumentTitle)", "Swagger UI" },
                { "%(StylesheetIncludes)", "" },
                { "%(DiscoveryPaths)", String.Join("|", discoveryPaths) },
                { "%(ValidatorUrl)", "" },
                { "%(CustomScripts)", "" },
                { "%(DocExpansion)", "none" },
                { "%(SupportedSubmitMethods)", "get|put|post|delete|options|head|patch" },
                { "%(OAuth2Enabled)", "false" },
                { "%(OAuth2ClientId)", "" },
                { "%(OAuth2ClientSecret)", "" },
                { "%(OAuth2Realm)", "" },
                { "%(OAuth2AppName)", "" },
                { "%(OAuth2ScopeSeperator)", " " },
                { "%(OAuth2AdditionalQueryStringParams)", "{}" },
				{ "%(ApiKeyName)", "api_key" },
				{ "%(ApiKeyIn)", "query" }
            };
            _rootUrlResolver = rootUrlResolver;

            MapPathsForSwaggerUiAssets();

            // Use some custom versions to support config and extensionless paths
            var thisAssembly = GetType().Assembly;
            CustomAsset("index", thisAssembly, "Swashbuckle.SwaggerUi.CustomAssets.index.html", isTemplate: true);
            CustomAsset("css/screen-css", thisAssembly, "Swashbuckle.SwaggerUi.CustomAssets.screen.css");
            CustomAsset("css/typography-css", thisAssembly, "Swashbuckle.SwaggerUi.CustomAssets.typography.css");

            // Map route-path for logos to the embedded png's
            CustomAsset("images/favicon-32x32-png", thisAssembly, "favicon-32x32.png");
            CustomAsset("images/favicon-16x16-png", thisAssembly, "favicon-16x16.png");
        }

        public void InjectStylesheet(Assembly resourceAssembly, string resourceName, string media = "screen", bool isTemplate = false)
        {
            var path = "ext/" + resourceName.Replace(".", "-");

            var stringBuilder = new StringBuilder(_templateParams["%(StylesheetIncludes)"]);
            stringBuilder.AppendLine("<link href='" + path + "' media='" + media + "' rel='stylesheet' type='text/css' />");
            _templateParams["%(StylesheetIncludes)"] = stringBuilder.ToString();

            CustomAsset(path, resourceAssembly, resourceName, isTemplate);
        }

        public void DocumentTitle(string title)
        {
            _templateParams["%(DocumentTitle)"] = title;
        }

        public void SetValidatorUrl(string url)
        {
            _templateParams["%(ValidatorUrl)"] = url;
        }

        public void DisableValidator()
        {
            _templateParams["%(ValidatorUrl)"] = "null";
        }

        public void InjectJavaScript(Assembly resourceAssembly, string resourceName, bool isTemplate = false)
        {
            var path = "ext/" + resourceName.Replace(".", "-");

            var stringBuilder = new StringBuilder(_templateParams["%(CustomScripts)"]);
            if (stringBuilder.Length > 0)
                stringBuilder.Append("|");

            stringBuilder.Append(path);
            _templateParams["%(CustomScripts)"] = stringBuilder.ToString();

            CustomAsset(path, resourceAssembly, resourceName, isTemplate);
        }

        public void DocExpansion(DocExpansion docExpansion)
        {
            _templateParams["%(DocExpansion)"] = docExpansion.ToString().ToLower();
        }

        public void SupportedSubmitMethods(params string[] methods)
        {
            _templateParams["%(SupportedSubmitMethods)"] = String.Join("|", methods).ToLower();
        }

        public void CustomAsset(string path, Assembly resourceAssembly, string resourceName, bool isTemplate = false)
        {
            if (path == "index") isTemplate = true;
            _pathToAssetMap[path] = new EmbeddedAssetDescriptor(resourceAssembly, resourceName, isTemplate);
        }

        public void EnableDiscoveryUrlSelector()
        {
            InjectJavaScript(GetType().Assembly, "Swashbuckle.SwaggerUi.CustomAssets.discoveryUrlSelector.js");
        }

        public void EnableOAuth2Support(string clientId, string realm, string appName)
        {
            EnableOAuth2Support(clientId, "N/A", realm, appName);
        }

        public void EnableOAuth2Support(
            string clientId,
            string clientSecret,
            string realm,
            string appName,
            string scopeSeperator = " ",
            Dictionary<string, string> additionalQueryStringParams = null)
        {
            _templateParams["%(OAuth2Enabled)"] = "true";
            _templateParams["%(OAuth2ClientId)"] = clientId;
            _templateParams["%(OAuth2ClientSecret)"] = clientSecret;
            _templateParams["%(OAuth2Realm)"] = realm;
            _templateParams["%(OAuth2AppName)"] = appName;
            _templateParams["%(OAuth2ScopeSeperator)"] = scopeSeperator;

            if (additionalQueryStringParams != null)
                _templateParams["%(OAuth2AdditionalQueryStringParams)"] = JsonConvert.SerializeObject(additionalQueryStringParams);
        }

		public void EnableApiKeySupport(string name, string apiKeyIn) {
			_templateParams["%(ApiKeyName)"] = name;
			_templateParams["%(ApiKeyIn)"] = apiKeyIn;
		}

        internal IAssetProvider GetSwaggerUiProvider()
        {
            return new EmbeddedAssetProvider(_pathToAssetMap, _templateParams);
        }

        internal string GetRootUrl(HttpRequestMessage swaggerRequest)
        {
            return _rootUrlResolver(swaggerRequest);
        }

        private void MapPathsForSwaggerUiAssets()
        {
            var thisAssembly = GetType().Assembly;
            foreach (var resourceName in thisAssembly.GetManifestResourceNames())
            {
                if (resourceName.Contains("Swashbuckle.SwaggerUi.CustomAssets")) continue; // original assets only

                var path = resourceName
                    .Replace("\\", "/")
                    .Replace(".", "-"); // extensionless to avoid RUMMFAR

                _pathToAssetMap[path] = new EmbeddedAssetDescriptor(thisAssembly, resourceName, path == "index");
            }
        }
    }

    public enum DocExpansion
    {
        None,
        List,
        Full
    }
}