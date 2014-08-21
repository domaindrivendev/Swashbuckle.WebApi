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
            
            var stream = embeddedResource.SupportsConfigExpressions
                ? ApplyConfigExpressions(embeddedResource.ToStream(), request)
                : embeddedResource.ToStream();

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

            return embeddedResource ?? new EmbeddedResource(GetType().Assembly, uiPath, uiPath == "index.html");
        }

        private Stream ApplyConfigExpressions(Stream stream, HttpRequestMessage request)
        {
            var text = new StreamReader(stream).ReadToEnd();
            var outputBuilder = new StringBuilder(text);

            var discoveryUrls = _swaggerSpecConfig.GetDiscoveryUrls(request);
            var listOfDiscoveryUrls = String.Join(",", discoveryUrls.Select(str => "'" + str + "'"));

            var listOfSubmitMethods = String.Join(",", _swaggerUiConfig.SupportedSubmitMethods.Select(str => "'" + str + "'"));

            outputBuilder
                .Replace("%(DiscoveryUrls)", "[" + listOfDiscoveryUrls + "]")
                .Replace("%(DefaultDiscoveryUrl)", "\"" + discoveryUrls.Last() + "\"")
                .Replace("%(SupportHeaderParams)", _swaggerUiConfig.SupportHeaderParams.ToString().ToLower())
                .Replace("%(SupportedSubmitMethods)", "[" + listOfSubmitMethods + "]")
                .Replace("%(DocExpansion)", "\"" + _swaggerUiConfig.DocExpansion.ToString().ToLower() + "\"");

            // Special cases - only applicable to index.html
            var stylesheetIncludes = String.Join("\r\n", _swaggerUiConfig.CustomEmbeddedResources.Values
                .Where(res => res.MediaType == "text/css")
                .Select(res => String.Format("<link href='ext/{0}' rel='stylesheet' type='text/css'/>", res.Name)));

            var scriptIncludes = String.Join("\r\n", _swaggerUiConfig.CustomEmbeddedResources.Values
                .Where(res => res.MediaType == "text/javascript")
                .Select(res => String.Format("$.getScript('ext/{0}');", res.Name)));

            outputBuilder
                .Replace("%(StylesheetIncludes)", stylesheetIncludes)
                .Replace("%(ScriptIncludes)", scriptIncludes);

            return new MemoryStream(Encoding.UTF8.GetBytes(outputBuilder.ToString()));
        }
    }
}