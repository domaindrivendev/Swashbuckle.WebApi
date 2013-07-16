using System.Linq;
using System.Web;
using System.Web.Routing;
using Swashbuckle.Models;

namespace Swashbuckle.Handlers
{
    public class SwaggerUiRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var swaggerUiConfig = SwaggerUiConfig.Instance;

            var routePath = requestContext.RouteData.Values["path"].ToString();
            var extensionScript = swaggerUiConfig.OnCompleteScripts.FirstOrDefault(s => s.RelativePath == routePath);

            var resourceAssembly = (extensionScript != null) ? extensionScript.ResourceAssembly : GetType().Assembly;
            var resourceName = (extensionScript != null) ? extensionScript.ResourceName : routePath;

            if (resourceName == "index.html")
            {
                var response = requestContext.HttpContext.Response;
                response.Filter = new SwaggerUiConfigFilter(response.Filter, swaggerUiConfig);
            }

            return new EmbeddedResourceHttpHandler(resourceAssembly, r => resourceName);
        }
    }
}