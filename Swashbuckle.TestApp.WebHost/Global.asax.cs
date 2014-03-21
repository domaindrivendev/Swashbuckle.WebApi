using System;
using System.Web;
using System.Web.Http;
using Swashbuckle.TestApp.Core;

namespace Swashbuckle.TestApp.WebHost
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var config = GlobalConfiguration.Configuration;

            WebApiConfig.Register(config);
            SwaggerConfig.Register(config);
        }
    }
}