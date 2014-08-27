using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Text;

namespace Swashbuckle.Application
{
    public class SwaggerUiHandler : HttpMessageHandler
    {
        private readonly SwaggerSpecConfig _swaggerSpecConfig;
        private readonly SwaggerUiConfig _swaggerUiConfig;

        public SwaggerUiHandler()
            : this(SwaggerSpecConfig.StaticInstance, SwaggerUiConfig.StaticInstance)
        {
        }

        public SwaggerUiHandler(SwaggerSpecConfig swaggerSpecConfig, SwaggerUiConfig config)
        {
            _swaggerSpecConfig = swaggerSpecConfig;
            _swaggerUiConfig = config;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uiPath = request.GetRouteData().Values["uiPath"].ToString();
            var embeddedResource = EmbeddedResourceFor(uiPath);
            
            var stream = embeddedResource.MediaType.StartsWith("text/")
                ? ApplyConfigExpressions(embeddedResource.GetStream(), request)
                : embeddedResource.GetStream();

            var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(embeddedResource.MediaType);

            var tsc = new TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(new HttpResponseMessage { Content = content });
            return tsc.Task;
        }
        
        private EmbeddedResource EmbeddedResourceFor(string uiPath)
        {
            EmbeddedResource embeddedResource;
            _swaggerUiConfig.CustomEmbeddedResources.TryGetValue(uiPath, out embeddedResource);

            return embeddedResource ?? new EmbeddedResource(GetType().Assembly, uiPath);
        }

        private Stream ApplyConfigExpressions(Stream stream, HttpRequestMessage request)
        {
            var text = new StreamReader(stream).ReadToEnd();
            var outputBuilder = new StringBuilder(text);

            var discoveryUrls = GetDiscoveryUrls(request);
            var discoveryUrlsString = String.Join(",", discoveryUrls
                .Select(url => "'" + url + "'"));

            var submitMethodsString = String.Join(",", _swaggerUiConfig.SupportedSubmitMethods
                .Select(method => "'" + method.ToString().ToLower() + "'"));

            var customScriptsString = String.Join(",", _swaggerUiConfig.CustomEmbeddedResources.Values
                .Where(res => res.MediaType == "text/javascript")
                .Select(res => "'" + res.Name + "'"));

            outputBuilder
                .Replace("%(DiscoveryUrls)", "[" + discoveryUrlsString + "]")
                .Replace("%(DefaultDiscoveryUrl)", "\"" + discoveryUrls.Last() + "\"")
                .Replace("%(SupportHeaderParams)", _swaggerUiConfig.SupportHeaderParams.ToString().ToLower())
                .Replace("%(SupportedSubmitMethods)", "[" + submitMethodsString + "]")
                .Replace("%(CustomScripts)", "[" + customScriptsString + "]")
                .Replace("%(DocExpansion)", "\"" + _swaggerUiConfig.DocExpansion.ToString().ToLower() + "\"")
                .Replace("%(OAuth2Enabled)", _swaggerUiConfig.OAuth2Enabled.ToString().ToLower())
                .Replace("%(OAuth2ClientId)", "\"" + _swaggerUiConfig.OAuth2ClientId + "\"")
                .Replace("%(OAuth2Realm)", "\"" + _swaggerUiConfig.OAuth2Realm + "\"")
                .Replace("%(OAuth2AppName)", "\"" + _swaggerUiConfig.OAuth2AppName + "\"");

            // Special case - only applicable to index.html
            var stylesheetIncludes = String.Join("\r\n", _swaggerUiConfig.CustomEmbeddedResources.Values
                .Where(res => res.MediaType == "text/css")
                .Select(res => String.Format("<link href='{0}' rel='stylesheet' type='text/css'/>", res.Name)));

            outputBuilder
                .Replace("%(StylesheetIncludes)", stylesheetIncludes);

            return new MemoryStream(Encoding.UTF8.GetBytes(outputBuilder.ToString()));
        }

        private IEnumerable<string> GetDiscoveryUrls(HttpRequestMessage swaggerRequest)
        {
            var basePath = _swaggerSpecConfig.BasePathResolver(swaggerRequest);
            if (_swaggerSpecConfig.Versions == null)
                return new[] { basePath + "/swagger/api-docs" };

            return _swaggerSpecConfig.Versions
                .Select(version => String.Format("{0}/swagger/{1}/api-docs", basePath, version));
        }
    }
}