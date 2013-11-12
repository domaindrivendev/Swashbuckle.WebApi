
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.Routing;


namespace Swashbuckle.WebApi.TestApp.App_Start
{
    public class SwaggerRoutes
    {
        public static void Register(HttpConfiguration config)
        {
           // //config.Routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

           
           // config.Routes.MapHttpRoute(
           //     "swagger_declaration",
           //     "swagger/api-docs/{resourceName}",
           //     new { controller = "ApiDocs", action = "Show" });

           // config.Routes.MapHttpRoute(
           //     "swagger_listing",
           //     "swagger/api-docs",
           //     new { controller = "ApiDocs", action = "Index" });

           // //config.Routes.Add("swagger",new HttpRoute(
           // //    "swagger",
           // //    null,
           // //    new HttpRouteValueDictionary(new {constraint= new RouteDirectionConstraint(RouteDirection.IncomingRequest)}),
           // //    null,
           // //    new RedirectRouteMessageHandler("http://localhost:58344/swagger/ui/index.html")));

           // config.Routes.MapHttpRoute(
           //    name: "Default",
           //    routeTemplate: "{controller}/{action}/{id}",
           //    defaults: new { controller = "Home", action = "Index", id = "Id" }
           //);


            //config.Routes.Add(new Route(
            //    "swagger/ui/{*path}",
            //    null,
            //    new RouteValueDictionary(new { constraint = new RouteDirectionConstraint(RouteDirection.IncomingRequest) }),
            //    new SwaggerUiRouteHandler()));
        }
    }
}