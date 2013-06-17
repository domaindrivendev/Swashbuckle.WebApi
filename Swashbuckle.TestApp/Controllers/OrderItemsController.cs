using System.Collections.Generic;
using System.Web.Http;
using Swashbuckle.TestApp.Models;

namespace Swashbuckle.TestApp.Controllers
{
    public class OrderItemsController : ApiController
    {
        public IEnumerable<OrderItem> GetAll(int orderId)
        {
            return new[]
                {
                    new OrderItem {LineNo = 1, Product = "Test Product 1", Quantity = 2},
                    new OrderItem {LineNo = 1, Product = "Test Product 2", Quantity = 4},
                    new OrderItem {LineNo = 1, Product = "Test Product 3", Quantity = 3}
                };
        }
    }
}