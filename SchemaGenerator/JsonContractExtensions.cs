using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace SchemaGenerator
{
    public static class JsonContractExtensions
    {
        private static IEnumerable<string> HttpTypeNames = new[]
            {
                "System.Net.Http.HttpRequestMessage",
                "System.Net.Http.HttpResponseMessage",
                "System.Web.Http.IHttpActionResult"
            };

        public static bool IsSelfReferencing(this JsonDictionaryContract dictionaryContract)
        {
            return dictionaryContract.UnderlyingType == dictionaryContract.DictionaryValueType;
        }

        public static bool IsSelfReferencing(this JsonArrayContract arrayContract)
        {
            return arrayContract.UnderlyingType == arrayContract.CollectionItemType;
        }

        public static bool IsInferrable(this JsonObjectContract objectContract)
        {
            return !HttpTypeNames.Contains(objectContract.UnderlyingType.FullName);
        }
    }
}