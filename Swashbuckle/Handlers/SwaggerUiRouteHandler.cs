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
            var includes =
                swaggerUiConfig.EmbeddedStylesheets.Union(swaggerUiConfig.OnCompleteScripts)
                               .FirstOrDefault(e => e.RelativePath == routePath);


            var resourceAssembly = includes != null ? includes.ResourceAssembly : GetType().Assembly;
            var resourceName = includes != null ? includes.ResourceName : routePath;

            if (resourceName == "index.html")
            {
                var response = requestContext.HttpContext.Response;
                response.Filter = new SwaggerUiConfigFilter(response.Filter, swaggerUiConfig);
            }

            return new EmbeddedResourceHttpHandler(resourceAssembly, r => resourceName);
        }
    }
}