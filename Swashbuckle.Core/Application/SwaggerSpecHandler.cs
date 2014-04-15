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
        {
            _config = SwaggerSpecConfig.StaticInstance;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var swaggerProvider = GetSwaggerProvider(request.GetConfiguration());
            
            var basePath = _config.ResolveBasePath(request);
            var version = _config.ResolveTargetVersion(request);

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

        private ISwaggerProvider GetSwaggerProvider(HttpConfiguration httpConfig)
        {
            var swaggerProvider = new ApiExplorerAdapter(
                httpConfig.Services.GetApiExplorer(),
                _config.IgnoreObsoleteActionsFlag,
                _config.ResolveVersionSupport,
                _config.ResolveResourceName,
                _config.PolymorphicTypes,
                _config.ModelFilters,
                _config.OperationFilters);

            return new CachingSwaggerProvider(swaggerProvider);
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