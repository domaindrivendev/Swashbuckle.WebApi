using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Net.Http;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class DynamicsController : ApiController
    {
		public int Create(dynamic anything)
        {
            throw new NotImplementedException();
        }

		public JObject GeByProperties(ExpandoObject anything)
        {
            throw new NotImplementedException();
        }

		public IEnumerable<JToken> GetAll()
        {
            throw new NotImplementedException();
        }

		public HttpResponseMessage Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
