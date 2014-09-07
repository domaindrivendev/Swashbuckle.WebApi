using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
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
    [TestFixture]
    public abstract class HttpMessageHandlerTestFixture<THandler>
        where THandler : HttpMessageHandler, new()
    {
        private string _routeTemplate;
        private THandler _handler;

        protected HttpMessageHandlerTestFixture(string routeTemplate) 
        {
            _routeTemplate = routeTemplate;
        }

        protected HttpConfiguration Configuration { get; private set; }

        [SetUp]
        public void BaseSetUp()
        {
            _handler = new THandler();
            Configuration = new HttpConfiguration();
        }

        protected void AddDefaultRoutesFor(IEnumerable<Type> controllerTypes)
        {
            foreach (var type in controllerTypes)
            {
                var controllerName = type.Name.ToLower().Replace("controller", String.Empty);
                var route = new HttpRoute(
                    String.Format("{0}/{{id}}", controllerName),
                    new HttpRouteValueDictionary(new { controller = controllerName, id = RouteParameter.Optional }));
                Configuration.Routes.Add(controllerName, route);
            }
        }

        protected void AddDefaultRouteFor<TController>()
            where TController : ApiController
        {
            AddDefaultRoutesFor(new[] { typeof(TController) });
        }
        
        protected void AddCustomRouteFor<TController>(string routeTemplate)
            where TController : ApiController
        {
            var controllerName = typeof(TController).Name.ToLower().Replace("controller", String.Empty);
            var route = new HttpRoute(
                routeTemplate,
                new HttpRouteValueDictionary(new { controller = controllerName, id = RouteParameter.Optional }));
            Configuration.Routes.Add(controllerName, route);
        }

        protected void AddAttributeRoutes()
        {
            Configuration.MapHttpAttributeRoutes();
            Configuration.EnsureInitialized();
        }

        protected TContent Get<TContent>(string uri)
        {
            var responseMessage = ExecuteGet(uri);
            return responseMessage.Content.ReadAsAsync<TContent>().Result;
        }

        protected string GetAsString(string uri)
        {
            var responseMessage = ExecuteGet(uri);
            Assert.That(responseMessage, Is.Not.Null, "responseMessage");
            Assert.That(responseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseMessage.Content, Is.Not.Null, "responseMessage.Content");
            return responseMessage.Content.ReadAsStringAsync().Result;
        }

        protected HttpResponseMessage ExecuteGet(string uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = Configuration;

            var route = new HttpRoute(_routeTemplate);
            var routeData = route.GetRouteData("/", request) ?? new HttpRouteData(route);

            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;

            return new HttpMessageInvoker(_handler)
                .SendAsync(request, new CancellationToken(false))
                .Result;
        }
    }
}