using System.Web.Http;
using Swashbuckle.App_Start;
using Swashbuckle.Models;

namespace Swashbuckle.TestApp.Api.App_Start
{
    public static class SwaggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            Bootstrapper.Init(config);

            SwaggerUiConfig.Customize(c =>
                c.AddOnCompleteScript(typeof(SwaggerConfig).Assembly, "Swashbuckle.TestApp.Api.SwaggerExtensions.customScript.js"));
        }
    }
}