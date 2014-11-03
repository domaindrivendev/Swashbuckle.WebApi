using System.Net.Http;
using System.Threading;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Headers;
using Swashbuckle.WebAssets;

namespace Swashbuckle.Application
{
    public class SwaggerUiHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, string> _rootUrlResolver;
        private readonly IWebAssetProvider _swaggerUiProvider;

        public SwaggerUiHandler(Func<HttpRequestMessage, string> rootUrlResolver, IWebAssetProvider swaggerUiProvider)
        {
            _rootUrlResolver = rootUrlResolver;
            _swaggerUiProvider = swaggerUiProvider;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uiPath = request.GetRouteData().Values["uiPath"].ToString();
            var rootUrl = _rootUrlResolver(request);

            try
            {
                var webAsset = _swaggerUiProvider.GetWebAssetFor(uiPath.ToString(), rootUrl);
                var content = ContentFor(webAsset);
                return TaskFor(new HttpResponseMessage { Content = content });
            }
            catch (WebAssetNotFound ex)
            {
                return TaskFor(request.CreateErrorResponse(HttpStatusCode.NotFound, ex));
            }
        }

        private HttpContent ContentFor(WebAsset webAsset)
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