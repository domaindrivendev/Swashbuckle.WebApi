using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.WebApi.TestApp.Models;

namespace Swashbuckle.WebApi.TestApp.Controller
{
    public class OrdersControllers : ApiController
    {
        public Order Post(Order order)
        {
            return order;
        }

        public IList<Order> GetAll()
        {
            return new[]
                {
                    new Order {Id = 1, Description = "TestOrder 1", Total = 10.0M},
                    new Order {Id = 2, Description = "TestOrder 2", Total = 20.0M}
                };
        }

        public IEnumerable<Order> GetByParams(string foo, string bar)
        {
            return new[]
                {
                    new Order {Id = 1, Description = "TestOrder 1", Total = 10.0M},
                    new Order {Id = 2, Description = "TestOrder 2", Total = 20.0M}
                };
        }

        public void Delete(int id)
        {
        }
    }
}