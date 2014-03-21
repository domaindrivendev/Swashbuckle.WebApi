using System.Web.Http;

namespace $rootnamespace$
{
    public class SwaggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // NOTE: Don't remove this line, it's required to wire-up the swagger routes 
            Swashbuckle.Core.Bootstrapper.Init(config);

            // NOTE: If you want to customize the generated swagger or UI, use SwaggerSpecConfig and/or SwaggerUiConfig here ...
        }
    }
}