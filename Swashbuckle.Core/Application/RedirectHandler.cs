using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Swashbuckle.Application
{
    public class RedirectHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, string> _hostNameResolver;
        private readonly string _redirectPath;

        public RedirectHandler(Func<HttpRequestMessage, string> hostNameResolver, string redirectPath)
        {
            _hostNameResolver = hostNameResolver;
            _redirectPath = redirectPath;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpConfig = request.GetConfiguration();
            var virtualPathRoot = httpConfig.VirtualPathRoot;

            var uriString = String.Format("{0}://{1}{2}/{3}",
                request.RequestUri.Scheme,
                _hostNameResolver(request),
                (virtualPathRoot != "/") ? virtualPathRoot : "",
                _redirectPath);

            var response = request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.Location = new Uri(uriString);

            var tsc = new TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(response);
            return tsc.Task;
        }
    }
}