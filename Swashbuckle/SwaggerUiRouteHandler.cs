using System;
using System.Web;
using System.Web.Routing;

namespace Swashbuckle
{
    public class SwaggerUiRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new SwaggerUiHttpHandler(requestContext.RouteData);
        }
    }

    public class SwaggerUiHttpHandler : IHttpHandler
    {
        private readonly RouteData _routeData;

        public SwaggerUiHttpHandler(RouteData routeData)
        {
            _routeData = routeData;
        }

        public void ProcessRequest(HttpContext context)
        {
            if (_routeData.Values["path"] == null)
            {
                context.Response.Redirect(context.Request.RawUrl + "/index.html");
                return;
            }

            var filePath = _routeData.Values["path"].ToString();
            var assembly = typeof (SwaggerUiHttpHandler).Assembly;
            var resourceName = String.Format("{0}.Content.{1}", assembly.GetName().Name, filePath.Replace("/", "."));
            var stream = GetType().Assembly.GetManifestResourceStream(resourceName);

            context.Response.Clear();
            context.Response.ContentType = "text/html";

            if (filePath.EndsWith(".css"))
                context.Response.ContentType = "text/css";
            else if (filePath.EndsWith(".js"))
                context.Response.ContentType = "text/css";

            stream.CopyTo(context.Response.OutputStream);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}