using System.Web.Mvc;
using Swashbuckle.Models;

namespace Swashbuckle.Controllers
{
    public class ApiDocsController : Controller
    {
        private readonly SwaggerSpec _swaggerSpec;

        public ApiDocsController()
        {
            _swaggerSpec = SwaggerGenerator.GetInstance().GenerateSpec();
        }

        public JsonResult Index()
        {
            return Json(_swaggerSpec.ResourceListing, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Show(string resourceName)
        {
            return Json(_swaggerSpec.ApiDeclarations[resourceName], JsonRequestBehavior.AllowGet);
        }
    }
}