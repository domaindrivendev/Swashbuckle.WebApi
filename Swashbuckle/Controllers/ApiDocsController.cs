using System.Web.Http;
using System.Web.Mvc;
using Swashbuckle.Models;

namespace Swashbuckle.Controllers
{
    public class ApiDocsController : Controller
    {
        private readonly SwaggerSpec _swaggerSpec;

        public ApiDocsController()
        {
            var apiExplorer = GlobalConfiguration.Configuration.Services.GetApiExplorer();
            _swaggerSpec = SwaggerGenerator.Instance.Generate(apiExplorer);
        }

        public JsonResult Index()
        {
            return Json(_swaggerSpec.Listing, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Show(string resourceName)
        {
            var resourcePath = "/swagger/api-docs/" + resourceName;
            return Json(_swaggerSpec.Declarations[resourcePath], JsonRequestBehavior.AllowGet);
        }
    }
}