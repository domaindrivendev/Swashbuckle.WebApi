using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Swashbuckle.Core.Swagger;

namespace Swashbuckle.Core.Application
{
    public class SwaggerSpecHandler : HttpMessageHandler
    {
        private static readonly object SyncRoot = new object();
        private static SwaggerSpec _cachedSwaggerSpec;

        private readonly SwaggerSpecConfig _config;

        public SwaggerSpecHandler()
        {
            _config = SwaggerSpecConfig.StaticInstance;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var swaggerSpec = GetSwaggerSpec(request);

            object declarationName;
            request.GetRouteData().Values.TryGetValue("declarationName", out declarationName);

            var responseMessage = (declarationName == null)
                ? ListingResponse(swaggerSpec)
                : DeclarationResponse(swaggerSpec, declarationName.ToString());

            return Task.Factory.StartNew(() => responseMessage);
        }

        private static HttpResponseMessage ListingResponse(SwaggerSpec swaggerSpec)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonTextFor(swaggerSpec.Listing), Encoding.UTF8, "application/json")
            };
        }

        private static HttpResponseMessage DeclarationResponse(SwaggerSpec swaggerSpec, string declarationName)
        {
            var declaration = swaggerSpec.Declarations["/" + declarationName];
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonTextFor(declaration), Encoding.UTF8, "application/json")
            };
        }

        private SwaggerSpec GetSwaggerSpec(HttpRequestMessage request)
        {
            lock (SyncRoot)
            {
                if (_cachedSwaggerSpec == null)
                {
                    var generator = new SwaggerGenerator(
                        _config.VersionResolver(request),
                        _config.BasePathResolver(request),
                        _config.IgnoreObsoleteActionsFlag,
                        _config.DeclarationKeySelector,
                        new OperationGenerator(
                            _config.OperationFilters,
                            new DataTypeGenerator(_config.CustomTypeMappings, _config.PolymorphicTypes)));

                    var apiExplorer = request.GetConfiguration().Services.GetApiExplorer();

                    _cachedSwaggerSpec = generator.ApiExplorerToSwaggerSpec(apiExplorer);
                }
            }

            return _cachedSwaggerSpec;
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