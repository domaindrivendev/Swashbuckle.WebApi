using System;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class PathRequiredController : ApiController
    {
        public void Get(long id = 0)
        {
            throw new NotImplementedException();
        }
    }
}
