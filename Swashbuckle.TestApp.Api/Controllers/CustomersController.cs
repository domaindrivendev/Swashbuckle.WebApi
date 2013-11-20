using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Swashbuckle.TestApp.Api.Models;

namespace Swashbuckle.TestApp.Api.Controllers
{
    public class CustomersController : ApiController
    {
        public JObject Post(JObject customer)
        {
            throw new NotImplementedException();
        }

        [Authorize]
        public Customer Get(int id)
        {
            return new Customer
                {
                    Id = id,
                };
        }

        [Authorize]
        public HttpResponseMessage Delete(int id)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
