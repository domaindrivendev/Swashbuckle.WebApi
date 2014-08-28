using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Swashbuckle.Swagger;
using System.Net.Http.Formatting;

namespace Swashbuckle.Application
{
    public class SwaggerSpecHandler : HttpMessageHandler
    {
        private readonly SwaggerSpecConfig _swaggerSpecConfig;

        public SwaggerSpecHandler()
            : this(SwaggerSpecConfig.StaticInstance)
        {}

        public SwaggerSpecHandler(SwaggerSpecConfig config)
        {
            _swaggerSpecConfig = config;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            object resourceName;
            request.GetRouteData().Values.TryGetValue("resourceName", out resourceName);

            try
            {
                var swaggerGenerator = _swaggerSpecConfig.GetGenerator(request);

                var content = (resourceName == null)
                    ? ContentFor(request, swaggerGenerator.GetListing())
                    : ContentFor(request, swaggerGenerator.GetDeclaration(resourceName.ToString()));

                return TaskFor(new HttpResponseMessage { Content = content });
            }
            catch (ApiDeclarationNotFoundException ex)
            {
                return TaskFor(request.CreateErrorResponse(HttpStatusCode.NotFound, ex));
            }
        }

        private HttpContent ContentFor<T>(HttpRequestMessage request, T swaggerObject)
        {
            var negotiator = request.GetConfiguration().Services.GetContentNegotiator();
            var result = negotiator.Negotiate(typeof(T), request, new [] { GetSwaggerJsonFormatter() });

            return new ObjectContent(typeof(T), swaggerObject, result.Formatter, result.MediaType);
        }

        private MediaTypeFormatter GetSwaggerJsonFormatter()
        {
            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            return new JsonMediaTypeFormatter { SerializerSettings = settings };
        }

        private Task<HttpResponseMessage> TaskFor(HttpResponseMessage response)
        {
            var tsc = new TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(response);
            return tsc.Task;
        }
    }
}