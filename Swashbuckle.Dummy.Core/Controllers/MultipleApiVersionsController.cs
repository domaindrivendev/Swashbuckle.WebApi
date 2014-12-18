using System;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class MultipleApiVersionsController : ApiController
    {
        [Route("{apiVersion:regex(1.0|2.0)}/todos")]
        public int Create([FromBody]string description)
        {
            throw new NotImplementedException();
        }

        [HttpPut]
        [Route("{apiVersion:regex(2.0)}/todos/{id}")]
        public void Update(int id, [FromBody]string description)
        {
            throw new NotImplementedException();
        }
    }
}
