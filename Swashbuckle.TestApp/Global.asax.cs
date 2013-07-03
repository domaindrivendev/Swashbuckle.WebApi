using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Swashbuckle.TestApp.App_Start;

namespace Swashbuckle.TestApp
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);

            SwaggerConfig.Customize();
        }
    }
}