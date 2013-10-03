using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Swashbuckle.Handlers;

namespace Swashbuckle
{
    public class SwaggerAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get { return "Swagger"; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.Routes.MapRoute(
                "Swagger_index",
                "swagger/api-docs",
                new {controller = "ApiDocs", action = "Index"});

            context.Routes.MapRoute(
                "Swagger_show",
                "swagger/api-docs/{resourceName}",
                new {controller = "ApiDocs", action = "Show" });

            context.Routes.Add(new Route(
                "swagger",
                null,
                new RouteValueDictionary(new {constraint = new RouteDirectionConstraint(RouteDirection.IncomingRequest)}),
                new RedirectRouteHandler("swagger/ui/index.html")));

            context.Routes.Add(new Route(
                "swagger/ui/{*path}",
                null,
                new RouteValueDictionary(new {constraint = new RouteDirectionConstraint(RouteDirection.IncomingRequest)}),
                new SwaggerUiRouteHandler()));
        }
    }

    public class RouteDirectionConstraint : IRouteConstraint
    {
        private readonly RouteDirection _direction;

        public RouteDirectionConstraint(RouteDirection direction)
        {
            _direction = direction;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName,
                          RouteValueDictionary values, RouteDirection routeDirection)
        {
            return routeDirection == _direction;
        }
    }
}