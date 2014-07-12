using System;
using System.Web.Http;
using System.Web.Http.Routing;

namespace Swashbuckle.Tests
{
    public static class HttpRouteCollectionExtensions
    {
		public static HttpRouteCollection Include<TController>(this HttpRouteCollection routes)
        {
			var controllerName = typeof(TController).Name.ToLower().Replace("controller", String.Empty);
			var route = new HttpRoute(
                String.Format("{0}/{{id}}", controllerName),
 				new HttpRouteValueDictionary(new { controller = controllerName, id = RouteParameter.Optional }));
            routes.Add(controllerName, route);

            return routes;
        }
    }
}
