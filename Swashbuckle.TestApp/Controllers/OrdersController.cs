using System.Collections.Generic;
using System.Web.Http;
using Swashbuckle.TestApp.Models;

namespace Swashbuckle.TestApp.Controllers
{
    public class OrdersController : ApiController
    {
        public Order Post(Order order)
        {
            return order;
        }

        public IEnumerable<Order> GetAll()
        {
            return new[]
                {
                    new Order {Id = 1, Description = "TestOrder 1", Total = 10.0M},
                    new Order {Id = 2, Description = "TestOrder 2", Total = 20.0M}
                };
        }

        public IEnumerable<Order> Get(string foo, string bar)
        {
            return new[]
                {
                    new Order {Id = 1, Description = "TestOrder 1", Total = 10.0M},
                    new Order {Id = 2, Description = "TestOrder 2", Total = 20.0M}
                };
        }
    }
}