using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Mvc;

namespace Swashbuckle.Dummy.Controllers
{
    public class ConflictingActionsController : ApiController
    {
        public IEnumerable<string> GetAll()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetAllByKeyword(string keyword)
        {
            throw new NotImplementedException();
        }
    }
}