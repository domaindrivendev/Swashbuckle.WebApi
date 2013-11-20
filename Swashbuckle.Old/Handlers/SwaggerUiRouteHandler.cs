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
            var resourceAssembly = GetType().Assembly;
            var resourceName = routePath;

            InjectedResourceDescriptor injectedResourceDescriptor;
            if (RequestIsForInjectedResource(routePath, swaggerUiConfig, out injectedResourceDescriptor))
            {
                resourceAssembly = injectedResourceDescriptor.ResourceAssembly;
                resourceName = injectedResourceDescriptor.ResourceName;
            }

            if (resourceName == "index.html")
            {
                var response = requestContext.HttpContext.Response;
                response.Filter = new SwaggerUiConfigFilter(response.Filter, swaggerUiConfig);
            }

            return new EmbeddedResourceHttpHandler(resourceAssembly, r => resourceName);
        }

        private bool RequestIsForInjectedResource(string routePath, SwaggerUiConfig swaggerUiConfig, out InjectedResourceDescriptor injectedResourceDescriptor)
        {
            injectedResourceDescriptor = swaggerUiConfig.CustomScripts
                .Union(swaggerUiConfig.CustomStylesheets)
                .FirstOrDefault(desc => desc.RelativePath == routePath);

            return injectedResourceDescriptor != null;
        }
    }
}