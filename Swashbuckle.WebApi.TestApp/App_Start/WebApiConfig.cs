using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Swashbuckle.WebApi.TestApp.App_Start
{
    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            config.Routes.MapHttpRoute(
                name: "OrdersApi",
                routeTemplate: "api/orders/{id}",
                defaults: new { controller = "Orders", id = RouteParameter.Optional }
                );

            config.Routes.MapHttpRoute(
                name: "OrderItemsApi",
                routeTemplate: "api/orders/{orderId}/items/{id}",
                defaults: new { controller = "OrderItems", id = RouteParameter.Optional }
                );

            config.Routes.MapHttpRoute(
                name: "CustomersApi",
                routeTemplate: "api/customers",
                defaults: new { controller = "Customers" }
                );
        }
    }
}