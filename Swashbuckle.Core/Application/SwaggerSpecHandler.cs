using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Swashbuckle.Swagger;

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
            var swaggerGenerator = _swaggerSpecConfig.GetGenerator(request);
            
            object resourceName;
            request.GetRouteData().Values.TryGetValue("resourceName", out resourceName);

            string json = (resourceName == null)
                ? JsonTextFor(swaggerGenerator.GetListing())
                : JsonTextFor(swaggerGenerator.GetDeclaration(resourceName.ToString()));

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var tsc = new TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(new HttpResponseMessage() { Content = content });
            return tsc.Task;
        }

        private static string JsonTextFor(object value)
        {
            var serializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };
            var jsonBuilder = new StringBuilder();
            using (var writer = new StringWriter(jsonBuilder))
            {
                serializer.Serialize(writer, value);
            }

            return jsonBuilder.ToString();
        }
    }
}