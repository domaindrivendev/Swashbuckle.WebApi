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
        private readonly SwaggerDocsConfig _swaggerDocsConfig;

        public SwaggerDocsHandler( SwaggerDocsConfig swaggerDocsConfig)
        {
            _swaggerDocsConfig = swaggerDocsConfig;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var swaggerProvider = GetSwaggerProvider(request);
            var apiVersion = request.GetRouteData().Values["apiVersion"].ToString();

            try
            {
                var swaggerDoc = swaggerProvider.GetSwaggerFor(apiVersion);
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
            var httpConfig = request.GetConfiguration();

            return new SwaggerGenerator(
                _swaggerDocsConfig.GetRootUrlResolver()(request),
                httpConfig.Services.GetApiExplorer(),
                httpConfig.GetJsonContractResolver(),
                _swaggerDocsConfig.GetGeneratorSettings());
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
                SerializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Converters = new[] { new VendorExtensionsConverter() }
                }
            };
            // NOTE: The custom converter would not be neccessary in Newtonsoft.Json >= 5.0.5 as JsonExtensionData
            // provides similar functionality. But, need to stick with older version for WebApi 5.0.0 compatibility 
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
