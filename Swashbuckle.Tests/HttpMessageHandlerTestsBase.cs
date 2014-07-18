using Newtonsoft.Json.Linq;
using Swashbuckle.Application;
using System;
using System.Collections.Generic;
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
        private HttpConfiguration _httpConfiguration;

		protected HttpMessageHandlerTestsBase(string routeTemplate)
        {
            _routeTemplate = routeTemplate;
            _httpConfiguration = new HttpConfiguration();
        }

        protected THandler Handler { get; set; }

        protected void SetUpDefaultRoutesFor(IEnumerable<Type> controllerTypes)
        {
            _httpConfiguration = new HttpConfiguration();

            foreach (var type in controllerTypes)
            {
                var controllerName = type.Name.ToLower().Replace("controller", String.Empty);
                var route = new HttpRoute(
                    String.Format("{0}/{{id}}", controllerName),
                    new HttpRouteValueDictionary(new { controller = controllerName, id = RouteParameter.Optional }));
                _httpConfiguration.Routes.Add(controllerName, route);
            }
        }

		protected void SetUpDefaultRouteFor<TController>()
			where TController : ApiController
        {
            SetUpDefaultRoutesFor(new[] { typeof(TController) });
        }
		
		protected void SetUpCustomRouteFor<TController>(string routeTemplate)
			where TController : ApiController
        {
            _httpConfiguration = new HttpConfiguration();

			var controllerName = typeof(TController).Name.ToLower().Replace("controller", String.Empty);
			var route = new HttpRoute(
				routeTemplate,
				new HttpRouteValueDictionary(new { controller = controllerName, id = RouteParameter.Optional }));
			_httpConfiguration.Routes.Add(controllerName, route);
        }

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
            if (Handler == null)
                throw new InvalidOperationException("Handler not set");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = _httpConfiguration; 

			var route = new HttpRoute(_routeTemplate);
            var routeData = route.GetRouteData("/", request) ?? new HttpRouteData(route);
			request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;

            return new HttpMessageInvoker(Handler)
                .SendAsync(request, new CancellationToken(false))
                .Result;
        }
    }
}
