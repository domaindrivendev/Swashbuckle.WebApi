using System.Collections.Generic;
using System.Web.Http;
using Swashbuckle.TestApp.Models;

namespace Swashbuckle.TestApp.Controllers
{
    public class OrderItemsController : ApiController
    {
        /// <summary>
        /// Get order item by id
        /// </summary>
        /// <param name="orderId">The unique identifier of an order</param>
        /// <param name="id">The unique identifier of something else</param>
        /// <remarks>Ok, we're not 100% sure what this thing does at all, use at your own risk</remarks>
        /// <returns></returns>
        public OrderItem GetById(int orderId, int id)
        {
            return new OrderItem {LineNo = id, Product = "Test Product 1", Quantity = 2};
        }

        /// <summary>
        /// Get all order items
        /// </summary>
        /// <remarks>Returns all three order items we've got here</remarks>
        /// <response code="200">OK</response>
        /// <response code="401">Unauthorized</response>
        /// <returns></returns>
        public IEnumerable<OrderItem> GetAll(int orderId, ProductCategory? category)
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