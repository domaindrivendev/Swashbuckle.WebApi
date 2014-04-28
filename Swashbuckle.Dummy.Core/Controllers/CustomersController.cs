using System;
using System.Web.Http;
using Swashbuckle.Dummy.Models;

namespace Swashbuckle.Dummy.Controllers
{
    public class CustomersController : ApiController
    {
        [Route("customers")]
        public int Create(Customer customer)
        {
            throw new NotImplementedException();
        }

        [Route("customers/{id}")]
        [HttpPut]
        public void Update(int id)
        {
            throw new NotImplementedException();
        }
    }
}