using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class CollectionOfPrimitivesController : ApiController
    {
        public int Create(IEnumerable<string> strings)
        {
            throw new NotImplementedException();
        }
    }
}