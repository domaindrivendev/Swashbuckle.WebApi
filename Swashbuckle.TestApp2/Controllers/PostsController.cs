using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Swashbuckle.TestApp2.Models;

namespace Swashbuckle.TestApp2.Controllers
{
    public class PostsController : ApiController
    {
        public IEnumerable<Post> GetAll()
        {
            throw new NotImplementedException();
        }

        [ResponseType(typeof(Confirmation))]
        public HttpResponseMessage Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
 