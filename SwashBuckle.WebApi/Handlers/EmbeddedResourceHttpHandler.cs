using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace SwashBuckle.WebApi.Handler
{
    public class EmbeddedResourceHttpHandler : DelegatingHandler
    {
        private readonly Assembly _resourceAssembly;
        private readonly Func<HttpContextBase, string> _resourceNameSelector;


        public EmbeddedResourceHttpHandler(
           Assembly resourceAssembly,
           Func<HttpContextBase, string> resourceNameSelector)
        {
            _resourceAssembly = resourceAssembly;
            _resourceNameSelector = resourceNameSelector;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            System.Threading.CancellationToken cancellationToken)
        {
            // Delegate to testable overload
            var response = ProcessRequest(new HttpContextWrapper(HttpContext.Current), request);

            var tsc = new TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(response);
            return tsc.Task;
        }


        public HttpResponseMessage ProcessRequest(HttpContextBase context, HttpRequestMessage request)
        {
            var response = request.CreateResponse(HttpStatusCode.OK);

            var resourceName = _resourceNameSelector(context);

            var resourceStream = _resourceAssembly.GetManifestResourceStream(resourceName);
            if (resourceStream == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
            }
            else
            {
                var requestPath = request.RequestUri.LocalPath;
                if (requestPath.EndsWith(".html"))
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
                else if (requestPath.EndsWith("css"))
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/css");
                else if (requestPath.EndsWith(".js"))
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/javascript");

                var buffer = new MemoryStream();
                resourceStream.CopyTo(buffer);
                response.Content = new StreamContent(buffer);
            }

            return response;
        }
    }
}
