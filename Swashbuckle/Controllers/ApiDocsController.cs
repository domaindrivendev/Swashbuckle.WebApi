using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Swashbuckle.Models;

namespace Swashbuckle.Controllers
{
    public class ApiDocsController : ApiController
    {
        private readonly SwaggerSpec _swaggerSpec;
        private readonly JsonSerializerSettings _serializerSettings;

        public ApiDocsController()
        {
            var apiExplorer = GlobalConfiguration.Configuration.Services.GetApiExplorer();
            _swaggerSpec = SwaggerGenerator.Instance.Generate(apiExplorer);

            _serializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };
        }

        public HttpResponseMessage GetListing()
        {
            var jsonContent = JsonConvert.SerializeObject(_swaggerSpec.Listing, _serializerSettings);

            return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonContent)
                };
        }

        public HttpResponseMessage GetDeclaration(string resourceName)
        {
            var resourcePath = "/" + resourceName;
            var jsonContent = JsonConvert.SerializeObject(_swaggerSpec.Declarations[resourcePath], _serializerSettings);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonContent)
            };
        }
    }
}