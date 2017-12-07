using System.Configuration;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Swashbuckle.Dummy.WebHost
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            bool isolateAreaSwaggers;
            bool.TryParse(ConfigurationManager.AppSettings["isolateAreaSwaggers"], out isolateAreaSwaggers);

            AreaRegistration.RegisterAllAreas();
            SwaggerConfig.Register(GlobalConfiguration.Configuration, isolateAreaSwaggers);
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}