using System.Web.Mvc;
using Swashbuckle.Models;

namespace Swashbuckle.Controllers
{
    public class ApiDocsController : Controller
    {
        private readonly ISwaggerSpec _swaggerSpec;

        public ApiDocsController()
        {
            _swaggerSpec = SwaggerSpecFactory.GetSpec();
        }

        public JsonResult Index()
        {
            return Json(_swaggerSpec.GetResourceListing(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Show(string resourceName)
        {
            var resourcePath = "/swagger/api-docs/" + resourceName;
            return Json(_swaggerSpec.GetApiDeclaration(resourcePath), JsonRequestBehavior.AllowGet);
        }
    }
}