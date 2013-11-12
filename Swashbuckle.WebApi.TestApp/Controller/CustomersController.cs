using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Swashbuckle.WebApi.TestApp.Controller
{
    public class CustomersController : ApiController
    {
        [Authorize]
        public HttpResponseMessage GetAll()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}