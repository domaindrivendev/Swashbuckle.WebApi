using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Linq;

namespace Swashbuckle.Application
{
    public class SwaggerUiConfig
    {
        internal static readonly SwaggerUiConfig StaticInstance = new SwaggerUiConfig();

        public SwaggerUiConfig()
        {
            SupportHeaderParams = false;
            SupportedSubmitMethods = new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put };
            DocExpansion = DocExpansion.None;
            CustomEmbeddedResources = new Dictionary<string, EmbeddedResource>();
            InjectedScriptPaths = new List<string>();
            InjectedStylesheetPaths = new List<string>();

            // Use Swashbuckle specific index.html
            CustomRoute("index.html", GetType().Assembly, "Swashbuckle.SwaggerExtensions.index.html");

            // Use Swashbuckle specific swagger-oauth.js because we need a slightly different callback url
            CustomRoute("lib/swagger-oauth.js", GetType().Assembly, "Swashbuckle.SwaggerExtensions.swagger-oauth.js");
        }

        public bool SupportHeaderParams { get; set; }
        public IEnumerable<HttpMethod> SupportedSubmitMethods { get; set; }
        public DocExpansion DocExpansion { get; set; }
        internal IDictionary<string, EmbeddedResource> CustomEmbeddedResources { get; private set; }
        internal IList<string> InjectedScriptPaths { get; private set; }
        internal IList<string> InjectedStylesheetPaths { get; private set; }

        internal bool OAuth2Enabled { get; private set; }
        internal string OAuth2AppName { get; private set; }
        internal string OAuth2Realm { get; private set; }
        internal string OAuth2ClientId { get; private set; }
        internal string OAuth2State { get; set; }
        internal string OAuth2Nonce { get; set; }

        public static void Customize(Action<SwaggerUiConfig> customize)
        {
            customize(StaticInstance);
        }

        public void EnableDiscoveryUrlSelector()
        {
            InjectJavaScript(GetType().Assembly, "Swashbuckle.SwaggerExtensions.discoveryUrlSelector.js");
        }

        public void InjectJavaScript(Assembly resourceAssembly, string resourceName)
        {
            CustomEmbeddedResources[resourceName] = new EmbeddedResource(
                resourceAssembly,
                resourceName,
                "text/javascript");
            InjectedScriptPaths.Add(resourceName);
        }

        public void InjectStylesheet(Assembly resourceAssembly, string resourceName)
        {
            CustomEmbeddedResources[resourceName] = new EmbeddedResource(
                resourceAssembly,
                resourceName,
                "text/css");
            InjectedStylesheetPaths.Add(resourceName);
        }

        public void CustomRoute(string uiPath, Assembly resourceAssembly, string resourceName)
        {
            CustomEmbeddedResources[uiPath] = new EmbeddedResource(resourceAssembly, resourceName);
        }

        public void EnableOAuth2Support(string clientId, string realm, string appName)
        {
            OAuth2Enabled = true;
            OAuth2ClientId = clientId;
            OAuth2Realm = realm;
            OAuth2AppName = appName;
        }

        /// <summary>
        /// Enable oauth with optional parameter for opendid connect
        /// </summary>
        /// <param name="clientId">OAuth 2.0 Client Identifier valid at the Authorization Server</param>
        /// <param name="realm">Optional indicate the scope of protection in the manner described in HTTP/1.1 [RFC2617]</param>
        /// <param name="appName"></param>
        /// <param name="state">Optional opaque value used to maintain state between the request and the callback. </param>
        /// <param name="nonce">String value used to associate a Client session with an ID Token, and to mitigate replay attacks</param>
        public void EnableOAuth2Support(string clientId, string realm, string appName, string state, string nonce)
        {
            OAuth2State = state;
            OAuth2Nonce = nonce;
            EnableOAuth2Support(clientId, realm, appName);
        }
    }

    public enum DocExpansion
    {
        None,
        List,
        Full
    }
}