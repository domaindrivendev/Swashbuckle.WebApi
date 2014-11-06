using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Net.Http;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class DynamicTypesController : ApiController
    {
        public int Create(dynamic thing)
        {
            throw new NotImplementedException();
        }

        public ExpandoObject GeByProperties(JObject thing)
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

        public IHttpActionResult Head()
        {
            throw new NotImplementedException();
        }
    }
}