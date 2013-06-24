using System.Web;
using System.Web.Routing;
using Swashbuckle.WebApi.Models;

namespace Swashbuckle.WebApi.Handlers
{
    public class SwaggerUiRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            if (requestContext.HttpContext.Request.Path == "/swagger/ui/index.html")
            {
                var response = requestContext.HttpContext.Response;
                response.Filter = new SwaggerUiConfigFilter(response.Filter, SwashbuckleConfig.Instance.SwaggerUiConfig);
            }

            return new EmbeddedResourceHttpHandler(GetType().Assembly, r => r.Path);
        }
    }
}