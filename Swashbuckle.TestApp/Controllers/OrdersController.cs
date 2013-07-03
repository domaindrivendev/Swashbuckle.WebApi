using System.Collections.Generic;
using System.Web.Http;
using Swashbuckle.TestApp.Models;

namespace Swashbuckle.TestApp.Controllers
{
    public class OrdersController : ApiController
    {
        public IEnumerable<Order> GetAll()
        {
            return new[]
                {
                    new Order {Id = 1, Description = "TestOrder 1"},
                    new Order {Id = 2, Description = "TestOrder 2"}
                };
        }

        public IEnumerable<Order> Get(string foo, string bar)
        {
            return new[]
                {
                    new Order {Id = 1, Description = "TestOrder 1"},
                    new Order {Id = 2, Description = "TestOrder 2"}
                };
        }
    }
}