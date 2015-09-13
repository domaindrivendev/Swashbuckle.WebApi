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
using Newtonsoft.Json.Converters;

namespace Swashbuckle.Application
{
    public class SwaggerDocsHandler : HttpMessageHandler
    {
        private readonly SwaggerDocsConfig _config;

        public SwaggerDocsHandler(SwaggerDocsConfig config)
        {
            _config = config;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var swaggerProvider = _config.GetSwaggerProvider(request);
            var rootUrl = _config.GetRootUrl(request);
            var apiVersion = request.GetRouteData().Values["apiVersion"].ToString();

            try
            {
                var swaggerDoc = swaggerProvider.GetSwagger(rootUrl, apiVersion);
                var content = ContentFor(request, swaggerDoc, swaggerProvider.GetOptions());
                return TaskFor(new HttpResponseMessage { Content = content });
            }
            catch (UnknownApiVersion ex)
            {
                return TaskFor(request.CreateErrorResponse(HttpStatusCode.NotFound, ex));
            }
        }

        private HttpContent ContentFor(HttpRequestMessage request, SwaggerDocument swaggerDoc, SwaggerGeneratorOptions options)
        {
            var negotiator = request.GetConfiguration().Services.GetContentNegotiator();
            var result = negotiator.Negotiate(typeof(SwaggerDocument), request, GetSupportedSwaggerFormatters(options));

            return new ObjectContent(typeof(SwaggerDocument), swaggerDoc, result.Formatter, result.MediaType);
        }

        private IEnumerable<MediaTypeFormatter> GetSupportedSwaggerFormatters(SwaggerGeneratorOptions options)
        {
            var jsonFormatter = new JsonMediaTypeFormatter
            {
                SerializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Converters = new List<JsonConverter>() { new VendorExtensionsConverter() }
                }
            };

	          if (options.DescribeAllEnumsAsStrings)
	          {
		            jsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter()
		            {
			              CamelCaseText = options.DescribeStringEnumsInCamelCase
		            });
	          }

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
