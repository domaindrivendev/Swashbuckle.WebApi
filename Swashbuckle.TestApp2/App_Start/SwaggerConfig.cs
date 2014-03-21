using System.Web.Http;
using Swashbuckle.TestApp2;
using WebActivatorEx;

[assembly: PostApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace Swashbuckle.TestApp2
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            // NOTE: Don't remove this line, it's required to wire-up the swagger routes 
            Swashbuckle.Core.Bootstrapper.Init(GlobalConfiguration.Configuration);

            // NOTE: If you want to customize the generated swagger or UI, use SwaggerSpecConfig and/or SwaggerUiConfig here ...
        }
    }
}