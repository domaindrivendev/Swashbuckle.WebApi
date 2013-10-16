using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Swashbuckle.TestApp.Api.Controllers
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