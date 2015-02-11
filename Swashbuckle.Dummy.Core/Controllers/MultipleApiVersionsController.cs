using System;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class MultipleApiVersionsController : ApiController
    {
        [Route("{apiVersion:regex(V1|V2)}/todos")]
        public int Create([FromBody]string description)
        {
            throw new NotImplementedException();
        }

        [HttpPut]
        [Route("{apiVersion:regex(V2)}/todos/{id}")]
        public void Update(int id, [FromBody]string description)
        {
            throw new NotImplementedException();
        }
    }
}