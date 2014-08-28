using System.IO;
using System.Net;
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

            try
            {
                var content = ContentFor(request, embeddedResource);
                return TaskFor(new HttpResponseMessage { Content = content });
            }
            catch (FileNotFoundException ex)
            {
                return TaskFor(request.CreateErrorResponse(HttpStatusCode.NotFound, ex));
            }
        }
        
        private EmbeddedResource EmbeddedResourceFor(string uiPath)
        {
            EmbeddedResource embeddedResource;
            _swaggerUiConfig.CustomEmbeddedResources.TryGetValue(uiPath, out embeddedResource);

            return embeddedResource ?? new EmbeddedResource(GetType().Assembly, uiPath);
        }

        private HttpContent ContentFor(HttpRequestMessage request, EmbeddedResource embeddedResource)
        {
            var stream = embeddedResource.GetStream();
            var content = embeddedResource.MediaType.StartsWith("text/")
                ? new StreamContent(ApplyConfigExpressions(stream, request))
                : new StreamContent(stream);

            content.Headers.ContentType = new MediaTypeHeaderValue(embeddedResource.MediaType);
            return content;
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

            var customScriptsString = String.Join(",", _swaggerUiConfig.InjectedScriptPaths
                .Select(path => "'" + path + "'"));

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
            var stylesheetIncludes = String.Join("\r\n", _swaggerUiConfig.InjectedStylesheetPaths
                .Select(path => String.Format("<link href='{0}' rel='stylesheet' type='text/css'/>", path)));

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

        private Task<HttpResponseMessage> TaskFor(HttpResponseMessage response)
        {
            var tsc = new TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(response);
            return tsc.Task;
        }
    }
}