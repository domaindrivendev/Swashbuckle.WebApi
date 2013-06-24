using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;

namespace Swashbuckle.WebApi.Models
{
    public enum DocExpansionMode
    {
        None,
        List,
        Full
    }

    public class SwaggerUiConfig
    {
        public SwaggerUiConfig()
        {
            ApiKeyName = "special-key";
            ApiKey = "special-key";
            SupportHeaderParams = false;
            SupportedSubmitMethods = new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put };
            DocExpansion = DocExpansionMode.None;
        }

        public string ApiKeyName { get; set; }
        public string ApiKey { get; set; }
        public bool SupportHeaderParams { get; set; }
        public IEnumerable<HttpMethod> SupportedSubmitMethods { get; set; }
        public DocExpansionMode DocExpansion { get; set; }
        public string OnCompleteScriptPath { get; set; }
    }
}