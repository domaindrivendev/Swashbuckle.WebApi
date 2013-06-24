using System;
using System.IO;
using System.Reflection;
using System.Web;

namespace Swashbuckle.WebApi.Handlers
{
    public class EmbeddedResourceHttpHandler : IHttpHandler
    {
        private readonly Assembly _resourceAssembly;
        private readonly Func<HttpRequest, string> _resourceNameSelector;

        public EmbeddedResourceHttpHandler(
            Assembly resourceAssembly,
            Func<HttpRequest, string> resourceNameSelector)
        {
            _resourceAssembly = resourceAssembly;
            _resourceNameSelector = resourceNameSelector;
        }

        public void ProcessRequest(HttpContext context)
        {
            var resourceName = _resourceNameSelector(context.Request);

            var resourceStream = _resourceAssembly.GetManifestResourceStream(resourceName);
            if (resourceStream == null)
                throw new FileNotFoundException(resourceName);

            context.Response.Clear();
            var requestPath = context.Request.Path;
            if (requestPath.EndsWith(".html"))
                context.Response.ContentType = "text/html";
            else if (requestPath.EndsWith("css"))
                context.Response.ContentType = "text/css";
            else if (requestPath.EndsWith(".js"))
                context.Response.ContentType = "text/javascript";

            resourceStream.CopyTo(context.Response.OutputStream);
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}