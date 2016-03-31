using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class DictionaryTypesController : ApiController
    {
        [Route("term-definitions")]
        public IDictionary<Term, string> GetTermDescriptions()
        {
            throw new NotImplementedException();
        }
    }

    public enum Term
    {
        TermA,
        TermB
    }
}
