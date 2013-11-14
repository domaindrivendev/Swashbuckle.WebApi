using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SwashBuckle.WebApi.Handler
{
    public class RedirectRouteMessageHandler : DelegatingHandler
    {
        private readonly string _redirectUrl;

        public RedirectRouteMessageHandler(string redirectUrl)
        {
            _redirectUrl = redirectUrl;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = request.CreateResponse(HttpStatusCode.Moved, "moved");
            response.Headers.Location = new Uri(_redirectUrl);

            var tsc = new TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(response);
            return tsc.Task;
        }

    }
}
