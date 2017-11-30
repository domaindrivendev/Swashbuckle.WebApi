using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Swashbuckle.Dummy.WebHost
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            SwaggerConfig.Register(GlobalConfiguration.Configuration);
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}