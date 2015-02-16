using System;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Swashbuckle.Dummy.Controllers
{
    public class JsonAnnotatedTypesController : ApiController
    {
        public int Create(JsonRequest request)
        {
            throw new NotImplementedException();
        } 
    }

    public class JsonRequest
    {
        [JsonIgnore]
        public string IgnoredProperty { get; set; }

        [JsonProperty("foobar")]
        public string CustomNamedProperty { get; set; }

        public Category Category { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Category
    {
        A = 2,
        B = 4 
    }
}