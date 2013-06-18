using System.Web;
using System.Web.Routing;

namespace Swashbuckle.WebApi
{
    public class RedirectRouteHandler : IRouteHandler
    {
        private readonly string _redirectUrl;

        public RedirectRouteHandler(string redirectUrl)
        {
            _redirectUrl = redirectUrl;
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new RedirectHttpHandler(_redirectUrl);
        }
    }

    public class RedirectHttpHandler : IHttpHandler
    {
        private readonly string _redirectUrl;

        public RedirectHttpHandler(string redirectUrl)
        {
            _redirectUrl = redirectUrl;
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Redirect(_redirectUrl);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}