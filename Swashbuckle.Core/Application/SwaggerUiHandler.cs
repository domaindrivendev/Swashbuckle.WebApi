using System.Net.Http;
using System.Threading;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Headers;
using Swashbuckle.SwaggerUi;

namespace Swashbuckle.Application
{
    public class SwaggerUiHandler : HttpMessageHandler
    {
        private readonly SwaggerUiConfig _swaggerUiConfig;

        public SwaggerUiHandler(
            SwaggerUiConfig swaggerUiConfig)
        {
            _swaggerUiConfig = swaggerUiConfig;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var swaggerUiProvider = GetSwaggerUiProvider(request);
            var assetPath = request.GetRouteData().Values["assetPath"].ToString();

            try
            {
                var webAsset = swaggerUiProvider.GetAssetFor(assetPath);
                var content = ContentFor(webAsset);
                return TaskFor(new HttpResponseMessage { Content = content });
            }
            catch (AssetNotFound ex)
            {
                return TaskFor(request.CreateErrorResponse(HttpStatusCode.NotFound, ex));
            }
        }

        private ISwaggerUiProvider GetSwaggerUiProvider(HttpRequestMessage request)
        {
            return new EmbeddedSwaggerUiProvider(
                _swaggerUiConfig.GetRootUrlResolver()(request),
                _swaggerUiConfig.GetUiProviderSettings());
        }

        private HttpContent ContentFor(Asset webAsset)
        {
            var content = new StreamContent(webAsset.Stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(webAsset.MediaType);
            return content;
        }

        private Task<HttpResponseMessage> TaskFor(HttpResponseMessage response)
        {
            var tsc = new TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(response);
            return tsc.Task;
        }
    }
}