using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Swashbuckle.TestApp.Controllers
{
    public class CustomersController : ApiController
    {
        public HttpResponseMessage GetAll()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}