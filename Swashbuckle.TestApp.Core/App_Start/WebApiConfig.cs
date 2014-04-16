using System.Web.Http;

namespace Swashbuckle.TestApp
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "Orders_route",
                routeTemplate: "api/orders/{id}",
                defaults: new {controller = "Orders", id = RouteParameter.Optional}
                );

            config.Routes.MapHttpRoute(
                name: "OrderItems_route",
                routeTemplate: "api/orders/{orderId}/items/{id}",
                defaults: new {controller = "OrderItems", id = RouteParameter.Optional}
                );

            config.Routes.MapHttpRoute(
                name: "Customers_route",
                routeTemplate: "api/customers/{id}",
                defaults: new {controller = "Customers", id = RouteParameter.Optional}
                );

            config.Routes.MapHttpRoute(
                name: "Products_route",
                routeTemplate: "api/products",
                defaults: new {controller = "Products"}
                );


            // Uncomment below to support documentation from Xml Comments
//            try
//            {
//                config.Services.Replace(typeof(IDocumentationProvider), new XmlCommentsDocumentationProvider(
//                    HttpContext.Current.Server.MapPath("~/bin/Swashbuckle.TestApp.xml")));
//            }
//            catch (FileNotFoundException)
//            {
//                throw new Exception("Please enable \"XML documentation file\" in project properties with default (bin\\Swashbuckle.TestApp.xml) value or edit value in App_Start\\Swashbuckle.TestApp.cs");
//            }
        }
    }
}