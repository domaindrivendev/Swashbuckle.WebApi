using System.Web.Http;
using System.Web.Routing;
using Swashbuckle.Core.Handlers;


[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(SwashBuckle.WebApi.Register.SwashBuckleConfig), "PreStart")]
[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(SwashBuckle.WebApi.Register.SwashBuckleConfig), "PostStart")]
namespace SwashBuckle.WebApi.Register
{
    public class SwashBuckleConfig
    {
        public static void PreStart()
        {
            RouteTable.Routes.MapHttpRoute(
                name: "swagger_declaration",
                routeTemplate: "swagger/api-docs/{resourceName}",
                defaults: new { controller = "ApiDocs", action = "Show" }
            );


            RouteTable.Routes.MapHttpRoute(
                name: "swagger_listing",
                routeTemplate: "swagger/api-docs",
                defaults: new { controller = "ApiDocs", action = "Index" }
            );


            RouteTable.Routes.Add(new Route(
                "swagger",
                null,
                new RouteValueDictionary(new { constraint = new RouteDirectionConstraint(RouteDirection.IncomingRequest) }),
                new RedirectRouteHandler("swagger/ui/index.html")));


            RouteTable.Routes.Add(new Route(
                "swagger/ui/{*path}",
                null,
                new RouteValueDictionary(new { constraint = new RouteDirectionConstraint(RouteDirection.IncomingRequest) }),
                new SwaggerUiRouteHandler())
                );
        }

        public static void PostStart()
        {
            //var config = GlobalConfiguration.Configuration;

            //var swaggerUiConfig = SwaggerUiConfig.Instance;
            //config.Filters.Add(new SwaggerUiConfigFilterAttribute(swaggerUiConfig));
        }
    }
}
