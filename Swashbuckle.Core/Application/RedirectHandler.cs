using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Swashbuckle.Application
{
    public class RedirectHandler : HttpMessageHandler
    {
        private readonly string _redirectPath;
        private readonly Func<HttpRequestMessage, string> _basePathResolver;

        public RedirectHandler(string redirectPath)
        {
            _redirectPath = redirectPath;
            _basePathResolver = SwaggerSpecConfig.StaticInstance.ResolveBasePath;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var redirectUri = String.Format("{0}/{1}", _basePathResolver(request).TrimEnd('/'), _redirectPath);

            var response = request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.Location = new Uri(redirectUri);

            return Task.Factory.StartNew(() => response);
        }
    }
}