using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json.Linq;
using Swashbuckle.Dummy.Models;

namespace Swashbuckle.Dummy.Controllers
{
    public class RandomStuffController : ApiController
    {
        [Route("kittens")]
        public JObject CreateKitten(JObject kitten)
        {
            throw new NotImplementedException();
        }

        [Route("unicorns")]
        [ResponseType(typeof(Unicorn))]
        public IHttpActionResult CreateUnicorn(object unicorn)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a list of unicorns filtered by property names
        /// </summary>
        /// <param name="propertyValues">A list of name/value pairs for filtering results</param>
        /// <returns></returns>
        [Route("unicorns")]
        public IEnumerable<Unicorn> GetUnicorns(Dictionary<string, string> propertyValues)
        {
            throw new NotImplementedException();
        }

        [Route("unicorns/{id}")]
        [Authorize]
        public HttpResponseMessage DeleteUnicorn(int id)
        {
            throw new NotImplementedException();
        }
    }
}