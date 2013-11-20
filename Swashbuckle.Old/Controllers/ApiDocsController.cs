using System.Web.Http;
using System.Web.Mvc;
using Newtonsoft.Json;
using Swashbuckle.Models;

namespace Swashbuckle.Controllers
{
    public class ApiDocsController : Controller
    {
        private readonly SwaggerSpec _swaggerSpec;
        private readonly JsonSerializerSettings _serializerSettings;

        public ApiDocsController()
        {
            var apiExplorer = GlobalConfiguration.Configuration.Services.GetApiExplorer();
            _swaggerSpec = SwaggerSpec.CreateFrom(apiExplorer);

            _serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public ContentResult Index()
        {
            var jsonContent = JsonConvert.SerializeObject(_swaggerSpec.Listing, _serializerSettings);
            return Content(jsonContent, "application/json");
        }

        public ContentResult Show(string resourceName)
        {
            var resourcePath = "/" + resourceName;
            var jsonContent = JsonConvert.SerializeObject(_swaggerSpec.Declarations[resourcePath], _serializerSettings);
            return Content(jsonContent, "application/json");
        }
    }
}