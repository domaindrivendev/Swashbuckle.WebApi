using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Swashbuckle.TestApp.Models;
using Swashbuckle.TestApp.SwaggerExtensions;

namespace Swashbuckle.TestApp.Controllers
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
        [ResponseType(typeof(Confirmation))]
        public HttpResponseMessage Delete(int id)
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}