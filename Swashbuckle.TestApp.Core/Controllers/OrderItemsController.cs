using System;
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
        /// <param name="orderId">The identifier for the order</param>
        /// <param name="id">The identifier for the requested order item</param>
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
        public IEnumerable<OrderItem> GetAll(int orderId, ProductCategory? category = null)
        {
            return new[]
                {
                    new OrderItem {LineNo = 1, Product = "Test Product 1", Quantity = 2},
                    new OrderItem {LineNo = 2, Product = "Test Product 2", Quantity = 4},
                    new OrderItem {LineNo = 3, Product = "Test Product 3", Quantity = 3}
                };
        }

        /// <summary>
        /// Retreive items in an order by property names and values
        /// </summary>
        /// <param name="orderId">The identifier for the order</param>
        /// <param name="propertyValues">Dictionary of property names and values</param>
        /// <returns></returns>
        [HttpPut]
        public IEnumerable<OrderItem> GetByPropertyValues(int orderId, Dictionary<string, string> propertyValues)
        {
            throw new NotImplementedException();
        }
    }
}