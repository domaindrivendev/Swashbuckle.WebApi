using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Models;
using Swashbuckle.TestApp.Api.SwaggerExtensions;

namespace Swashbuckle.TestApp.Api.App_Start
{
    public class SwaggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // NOTE: This line is required to wire-up the swagger routes 
            Swashbuckle.Bootstrapper.Init(config);

            SwaggerSpecConfig.Customize(c =>
                {
                    c.PostFilter<AddStandardErrorCodes>();
                    c.PostFilter<AddAuthorizationErrorCodes>();

                    // Uncomment below to support documentation from Xml Comments
                    // c.PostFilter(new ExtractXmlComments());
                });

            SwaggerUiConfig.Customize(c =>
                {
                    c.SupportHeaderParams = true;
                    c.DocExpansion = DocExpansion.List;
                    c.SupportedSubmitMethods = new[] {HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Head};
                    c.AddOnCompleteScript(typeof (SwaggerConfig).Assembly, "Swashbuckle.TestApp.Api.SwaggerExtensions.onComplete.js");
                    c.AddStylesheet(typeof(SwaggerConfig).Assembly, "Swashbuckle.TestApp.Api.SwaggerExtensions.customStyles.css");
                });
        }
    }
}
