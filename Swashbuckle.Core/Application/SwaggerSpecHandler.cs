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
        private readonly SwaggerSpecConfig _config;

        public SwaggerSpecHandler()
            : this(SwaggerSpecConfig.StaticInstance)
        {}

        public SwaggerSpecHandler(SwaggerSpecConfig config)
        {
            _config = config;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var swaggerProvider = _config.GetSwaggerProvider(request.GetConfiguration().Services.GetApiExplorer());
            
            var basePath = _config.BasePathResolver(request);
            var version = _config.TargetVersionResolver(request);

            object resourceName;
            request.GetRouteData().Values.TryGetValue("resourceName", out resourceName);

            var content = (resourceName == null)
                ? ContentFor(swaggerProvider.GetListing(basePath, version))
                : ContentFor(swaggerProvider.GetDeclaration(basePath, version, resourceName.ToString()));

            return Task.Factory.StartNew(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            });
        }

        private HttpContent ContentFor(object value)
        {
            return new StringContent(JsonTextFor(value), Encoding.UTF8, "application/json");
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