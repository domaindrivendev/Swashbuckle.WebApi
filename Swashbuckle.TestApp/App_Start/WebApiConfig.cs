using System.Web.Http;

namespace Swashbuckle.TestApp.App_Start
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
        }
    }
}