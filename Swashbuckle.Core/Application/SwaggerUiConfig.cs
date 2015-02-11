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
        private readonly Dictionary<string, EmbeddedAssetDescriptor> _pathToAssetMap;
        private readonly Dictionary<string, string> _templateParams;
        private readonly Func<HttpRequestMessage, string> _rootUrlResolver;

        public SwaggerUiConfig(IEnumerable<string> discoveryPaths, Func<HttpRequestMessage, string> rootUrlResolver)
        {
            _pathToAssetMap = new Dictionary<string, EmbeddedAssetDescriptor>();

            // Avoid RAMMFAR by serving swagger-ui through extentionless URL's
            MapAssets(new Dictionary<string, string>
                {
                    { "index", "Swashbuckle.SwaggerUi.Assets.index.html" }, // SB version
                    { "o2c-html", "o2c.html" }, // SB version
                    { "swagger-ui-js", "swagger-ui.js" },
                    { "swagger-ui-min-js", "swagger-ui.min.js" },
                    { "css/reset-css", "css/reset.css" },
                    { "css/screen-css", "Swashbuckle.SwaggerUi.Assets.screen.css" }, // SB version
                    { "lib/shred/content-js", "lib/shred/content.js" },
                    { "lib/backbone-min-js", "lib/backbone-min.js" },
                    { "lib/handlebars-1-0-0-js", "lib/handlebars-1.0.0.js" },
                    { "lib/highlight-7-3-pack-js", "lib/highlight.7.3.pack.js" },
                    { "lib/jquery-1-8-0-min-js", "lib/jquery-1.8.0.min.js" },
                    { "lib/jquery-ba-bbq-min-js", "lib/jquery.ba-bbq.min.js" },
                    { "lib/jquery-slideto-min-js", "lib/jquery.slideto.min.js" },
                    { "lib/jquery-wiggle-min-js", "lib/jquery.wiggle.min.js" },
                    { "lib/shred-bundle-js", "lib/shred.bundle.js" },
                    { "lib/swagger-client-js", "lib/swagger-client.js" },
                    { "lib/swagger-oauth-js", "Swashbuckle.SwaggerUi.Assets.swagger-oauth.js" }, // SB version
                    { "lib/swagger-js", "lib/swagger.js" },
                    { "lib/underscore-min-js", "lib/underscore-min.js" },
                    { "images/explorer_icons-png", "images/explorer_icons.png" },
                    { "images/logo_small-png", "images/logo_small.png" },
                    { "images/pat_store_api-png", "images/pet_store_api.png" },
                    { "images/throbber-gif", "images/throbber.gif" },
                    { "images/wordnik_api-png", "images/wordnik_api.png"}
                }
            );

            _templateParams = new Dictionary<string, string>
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
        }

        public void InjectStylesheet(Assembly resourceAssembly, string resourceName)
        {
            var path = "ext/" + resourceName.Replace(".", "-");
            var stringBuilder = new StringBuilder(_templateParams["%(StylesheetIncludes)"]);

            stringBuilder.AppendLine("<link href='" + path + "' media='screen' rel='stylesheet' type='text/css' />");
            _templateParams["%(StylesheetIncludes)"] = stringBuilder.ToString();

            _pathToAssetMap[path] = new EmbeddedAssetDescriptor(resourceAssembly, resourceName);
        }
        
        public void BooleanValues(IEnumerable<string> values)
        {
            _templateParams["%(BooleanValues)"] = String.Join("|", values);
        }

        public void InjectJavaScript(Assembly resourceAssembly, string resourceName)
        {
            var stringBuilder = new StringBuilder(_templateParams["%(CustomScripts)"]);

            if (stringBuilder.Length > 0)
                stringBuilder.Append("|");

            var path = "ext/" + resourceName.Replace(".", "-");
            stringBuilder.Append(path);

            _templateParams["%(CustomScripts)"] = stringBuilder.ToString();
            _pathToAssetMap[path] = new EmbeddedAssetDescriptor(resourceAssembly, resourceName);
        }

        public void DocExpansion(DocExpansion docExpansion)
        {
            _templateParams["%(DocExpansion)"] = docExpansion.ToString().ToLower();
        }

        public void CustomAsset(string path, Assembly resourceAssembly, string resourceName)
        {
            _pathToAssetMap[path] = new EmbeddedAssetDescriptor(resourceAssembly, resourceName, path == "index");
        }

        public void EnableDiscoveryUrlSelector()
        {
            InjectJavaScript(GetType().Assembly, "Swashbuckle.SwaggerUi.Assets.discoveryUrlSelector.js");
        }

        public void EnableOAuth2Support(string clientId, string realm, string appName)
        {
            _templateParams["%(OAuth2Enabled)"] = "true";
            _templateParams["%(OAuth2ClientId)"] = clientId;
            _templateParams["%(OAuth2Realm)"] = realm;
            _templateParams["%(OAuth2AppName)"] = appName;
        }

        internal IAssetProvider GetSwaggerUiProvider()
        {
            return new EmbeddedAssetProvider(_pathToAssetMap, _templateParams);
        }

        internal string GetRootUrl(HttpRequestMessage swaggerRequest)
        {
            return _rootUrlResolver(swaggerRequest);
        }

        private void MapAssets(Dictionary<string, string> assetRoutes)
        {
            var resourceAssembly = GetType().Assembly;
            foreach (var entry in assetRoutes)
            {
                var path = entry.Key;
                var resourceName = entry.Value;
                CustomAsset(path, resourceAssembly, resourceName);
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