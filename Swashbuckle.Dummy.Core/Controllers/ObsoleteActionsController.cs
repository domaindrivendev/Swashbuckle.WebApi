using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class ObsoleteActionsController : ApiController
    {
        [HttpPut]
        public int Update(int id, string value)
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}