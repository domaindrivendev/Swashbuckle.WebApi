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
            var requestPath = string.Format("/swagger/ui/{0}", requestContext.RouteData.Values["path"]);

            // Check if it's the path to a configured extensibility script 
            var scriptDescriptor = swaggerUiConfig.OnCompleteScripts.SingleOrDefault(d => d.Path == requestPath);
            if (scriptDescriptor != null)
                return new EmbeddedResourceHttpHandler(scriptDescriptor.ResourceAssembly, r => scriptDescriptor.ResourceName);

            if (requestPath == "/swagger/ui/index.html")
            {
                var response = requestContext.HttpContext.Response;
                response.Filter = new SwaggerUiConfigFilter(response.Filter, swaggerUiConfig);
            }

            return new EmbeddedResourceHttpHandler(GetType().Assembly, r => requestPath);
        }
    }
}