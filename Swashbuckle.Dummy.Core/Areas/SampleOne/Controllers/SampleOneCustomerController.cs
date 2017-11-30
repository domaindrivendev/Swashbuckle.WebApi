using System;
using System.Web.Http;

namespace Swashbuckle.Dummy.Areas.SampleOne.Controllers
{
    [System.Web.Http.RoutePrefix("sampleone/api")]
    public class SampleOneCustomerController : ApiController
    {
        public int Create(Customer customer)
        {
            throw new NotImplementedException();
        }

        [System.Web.Http.HttpPut]
        [Route("")]
        public string Update(int id, Customer customer)
        {
            return $"customer {id} updated";
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        [Route("")]
        public Customer Get(int id)
        {
            return new Customer
            {
                Id = id,
                Name = $"Customer {id}"
            };
        }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
