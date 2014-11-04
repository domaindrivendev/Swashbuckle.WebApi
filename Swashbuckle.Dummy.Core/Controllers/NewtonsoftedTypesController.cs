using System;
using System.Web.Http;
using Newtonsoft.Json;

namespace Swashbuckle.Dummy.Controllers
{
    public class NewtonsoftedTypesController : ApiController
    {
        public int Create(NewtonsoftedModel model)
        {
            throw new NotImplementedException();
        } 
    }

    public class NewtonsoftedModel
    {
        [JsonIgnore]
        public string IgnoredProperty { get; set; }

        [JsonProperty("myCustomNamedProperty")]
        public string CustomNamedProperty { get; set; }
    }
}