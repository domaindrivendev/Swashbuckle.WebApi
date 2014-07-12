using Newtonsoft.Json.Linq;
using Swashbuckle.Application;
using System;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Swashbuckle.Tests
{
    public class HttpMessageHandlerTestsBase<THandler>
		where THandler : HttpMessageHandler
    {
        private string _routeTemplate;

		protected HttpMessageHandlerTestsBase(string routeTemplate)
        {
            _routeTemplate = routeTemplate;
        }

        protected HttpConfiguration HttpConfiguration { get; set; }

        protected THandler Handler { get; set; }

		protected TContent Get<TContent>(string uri)
        {
			var responseMessage = ExecuteGet(uri);
            return responseMessage.Content.ReadAsAsync<TContent>().Result;
        }

		protected string GetAsString(string uri)
        {
			var responseMessage = ExecuteGet(uri);
            return responseMessage.Content.ReadAsStringAsync().Result;
        }

		private HttpResponseMessage ExecuteGet(string uri)
        {
            if (HttpConfiguration == null)
                throw new InvalidOperationException("HttpConfiguration not set");
            if (Handler == null)
                throw new InvalidOperationException("Handler not set");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = HttpConfiguration; 

			var route = new HttpRoute(_routeTemplate);
            var routeData = route.GetRouteData("/", request) ?? new HttpRouteData(route);
			request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;

            return new HttpMessageInvoker(Handler)
                .SendAsync(request, new CancellationToken(false))
                .Result;
        }
    }
}
