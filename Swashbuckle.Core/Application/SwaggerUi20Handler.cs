using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Headers;
using Swashbuckle.WebAssets;

namespace Swashbuckle.Application
{
    public class SwaggerUi20Handler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            object uiPath;
            request.GetRouteData().Values.TryGetValue("uiPath", out uiPath);

            try
            {
                var webAsset = request.SwaggerUiProvider().GetWebAssetFor(uiPath.ToString());
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