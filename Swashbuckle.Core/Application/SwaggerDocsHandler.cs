using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using Swashbuckle.Swagger;
using System.Net;

namespace Swashbuckle.Application
{
    public class SwaggerDocsHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, string> _hostNameResolver;
        private readonly SwaggerDocsConfig _swaggerDocsConfig;

        public SwaggerDocsHandler(Func<HttpRequestMessage, string> hostNameResolver, SwaggerDocsConfig swaggerDocsConfig)
        {
            _hostNameResolver = hostNameResolver;
            _swaggerDocsConfig = swaggerDocsConfig;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var swaggerProvider = GetSwaggerProvider(request);

            object apiVersion;
            request.GetRouteData().Values.TryGetValue("apiVersion", out apiVersion);

            try
            {
                var swaggerDoc = swaggerProvider.GetSwaggerFor(apiVersion.ToString());
                var content = ContentFor(request, swaggerDoc);
                return TaskFor(new HttpResponseMessage { Content = content });
            }
            catch (UnknownApiVersion ex)
            {
                return TaskFor(request.CreateErrorResponse(HttpStatusCode.NotFound, ex));
            }
        }

        private ISwaggerProvider GetSwaggerProvider(HttpRequestMessage request)
        {
            var hostName = _hostNameResolver(request);
            var virtualPathRoot = request.GetConfiguration().VirtualPathRoot;
            var apiExplorer = request.GetConfiguration().Services.GetApiExplorer();

            var settings = _swaggerDocsConfig.ToGeneratorSettings();

            return new SwaggerGenerator(hostName, virtualPathRoot, apiExplorer, settings);
        }

        private HttpContent ContentFor(HttpRequestMessage request, SwaggerDocument swaggerDoc)
        {
            var negotiator = request.GetConfiguration().Services.GetContentNegotiator();
            var result = negotiator.Negotiate(typeof(SwaggerDocument), request, GetSupportedSwaggerFormatters());

            return new ObjectContent(typeof(SwaggerDocument), swaggerDoc, result.Formatter, result.MediaType);
        }

        private IEnumerable<MediaTypeFormatter> GetSupportedSwaggerFormatters()
        {
            var jsonFormatter = new JsonMediaTypeFormatter
            {
                SerializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
            };
            return new[] { jsonFormatter };
        }

        private Task<HttpResponseMessage> TaskFor(HttpResponseMessage response)
        {
            var tsc = new TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(response);
            return tsc.Task;
        }
    }
}
