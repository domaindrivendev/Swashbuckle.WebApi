using System.Linq;
using System.Web;
using System.Web.Routing;
using Swashbuckle.WebApi.Models;

namespace Swashbuckle.WebApi.Handlers
{
    public class SwaggerUiRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            var swaggerUiConfig = SwaggerUiConfig.Instance;
            var requestPath = requestContext.HttpContext.Request.Path;

            // Check if it's the path to a configured extensibility script 
            var scriptDescriptor = swaggerUiConfig.OnCompleteScripts.SingleOrDefault(d => d.Path == requestPath);
            if (scriptDescriptor != null)
                return new EmbeddedResourceHttpHandler(scriptDescriptor.ResourceAssembly, r => scriptDescriptor.ResourceName);

            if (requestContext.HttpContext.Request.Path == "/swagger/ui/index.html")
            {
                var response = requestContext.HttpContext.Response;
                response.Filter = new SwaggerUiConfigFilter(response.Filter, swaggerUiConfig);
            }

            return new EmbeddedResourceHttpHandler(GetType().Assembly, r => r.Path);
        }
    }
}