using System.Web.Http;

namespace Swashbuckle.TestApp.Api.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "OrdersApi",
                routeTemplate: "api/orders/{id}",
                defaults: new {controller = "Orders", id = RouteParameter.Optional}
                );

            config.Routes.MapHttpRoute(
                name: "OrderItemsApi",
                routeTemplate: "api/orders/{orderId}/items/{id}",
                defaults: new {controller = "OrderItems", id = RouteParameter.Optional}
                );

            config.Routes.MapHttpRoute(
                name: "CustomersApi",
                routeTemplate: "api/customers",
                defaults: new {controller = "Customers"}
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
