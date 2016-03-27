using Swashbuckle.Dummy.SwaggerExtensions;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Swashbuckle.Dummy.Controllers
{
    public class DictionaryController : ApiController
    {
        [HttpGet]
        public IEnumerable<ItemWithDictionary> Get()
        {
            throw new NotImplementedException();
        }
    }
    
    public class ItemWithDictionary
    {
        public IDictionary<KeyType, SimpleValue> Values { get; set; }	
    }

    public class SimpleValue
    {
        public string Name { get; set; }
    }

    public enum KeyType
    {
        KeyOne,
        KeyTwo,
    }
}