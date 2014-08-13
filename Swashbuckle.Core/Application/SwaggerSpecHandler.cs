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
                ? ContentFor(request, swaggerProvider.GetListing(basePath, version))
                : ContentFor(request, swaggerProvider.GetDeclaration(basePath, version, resourceName.ToString()));

            var tcs = new TaskCompletionSource<HttpResponseMessage>();
            tcs.SetResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            });
            return tcs.Task;
        }

        private HttpContent ContentFor<T>(HttpRequestMessage request, T value)
        {
            var negotiator = request.GetConfiguration().Services.GetContentNegotiator();
            var result = negotiator.Negotiate(typeof (T), request, request.GetConfiguration().Formatters);
            return new ObjectContent(typeof(T), value, result.Formatter, result.MediaType);
        }
    }
}