using Newtonsoft.Json.Linq;
using System;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class ProtectedResourcesController : ApiController
    {
        [Authorize(Roles = "read")]
        public JObject Get(int id)
        {
            throw new NotImplementedException();
        }
    }
}