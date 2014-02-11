using System.Web.Http;
using System.Web.Mvc;
using Newtonsoft.Json;
using Swashbuckle.Models;

namespace Swashbuckle.Controllers
{
    public class ApiDocsController : Controller
    {
        private readonly SwaggerGenerator _swaggerGenerator;
        private readonly JsonSerializerSettings _serializerSettings;

        public ApiDocsController()
        {
            var config = SwaggerSpecConfig.Instance;

            _swaggerGenerator = new SwaggerGenerator(
                config.IgnoreObsoleteActions,
                config.BasePathResolver,
                config.DeclarationKeySelector,
                config.CustomTypeMappings,
                config.OperationFilters,
                config.OperationSpecFilters);

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
            // TODO: implement caching - should only be generated once

            var apiExplorer = GlobalConfiguration.Configuration.Services.GetApiExplorer();
            return _swaggerGenerator.ApiExplorerToSwaggerSpec(apiExplorer);
        }
    }
}