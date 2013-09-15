using System.Collections.Generic;
using System.Web.Http;
using Swashbuckle.TestApp.Models;

namespace Swashbuckle.TestApp.Controllers
{
    public class OrderItemsController : ApiController
    {
        public OrderItem GetById(int orderId, int id)
        {
            return new OrderItem {LineNo = id, Product = "Test Product 1", Quantity = 2};
        }

        public IEnumerable<OrderItem> GetAll(int orderId, ProductCategory? category = null)
        {
            return new[]
                {
                    new OrderItem {LineNo = 1, Product = "Test Product 1", Quantity = 2},
                    new OrderItem {LineNo = 2, Product = "Test Product 2", Quantity = 4},
                    new OrderItem {LineNo = 3, Product = "Test Product 3", Quantity = 3}
                };
        }
    }
}