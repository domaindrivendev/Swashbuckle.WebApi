using System.Web.Http;
using System.Web.Mvc;
using Newtonsoft.Json;
using Swashbuckle.Models;

namespace Swashbuckle.Controllers
{
    public class ApiDocsController : Controller
    {
        private static readonly object SyncRoot = new object();
        private static SwaggerSpec _swaggerSpec;

        private readonly JsonSerializerSettings _serializerSettings;

        public ApiDocsController()
        {
            _serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public ContentResult Index()
        {
            var jsonContent = JsonConvert.SerializeObject(SwaggerSpec().Listing, _serializerSettings);
            return Content(jsonContent, "application/json");
        }

        public ContentResult Show(string resourceName)
        {
            var resourcePath = "/" + resourceName;
            var jsonContent = JsonConvert.SerializeObject(SwaggerSpec().Declarations[resourcePath], _serializerSettings);
            return Content(jsonContent, "application/json");
        }

        public SwaggerSpec SwaggerSpec()
        {
            lock (SyncRoot)
            {
                if (_swaggerSpec == null)
                {
                    var swaggerGenerator = new SwaggerGenerator();
                    var apiExplorer = GlobalConfiguration.Configuration.Services.GetApiExplorer();
                    _swaggerSpec = swaggerGenerator.ApiExplorerToSwaggerSpec(apiExplorer);
                }
            }

            return _swaggerSpec;
        }
    }
}