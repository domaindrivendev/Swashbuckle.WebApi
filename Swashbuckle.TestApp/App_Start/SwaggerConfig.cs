using System.Net.Http;
using Swashbuckle.Models;
using Swashbuckle.TestApp.SwaggerFilters;

namespace Swashbuckle.TestApp.App_Start
{
    public class SwaggerConfig
    {
        public static void Customize()
        {
            SwaggerSpecConfig.Customize(c =>
                {
                    c.PostFilter(new AddErrorCodeFilter(200, "It's all good!"));
                    c.PostFilter(new AddErrorCodeFilter(400, "Something's up!"));
                    // uncomment this to parse documentation field when using XmlCommentDocumentationProvider
                    //c.PostFilter(new AddXmlCommentsParsingFilter());
                });

            SwaggerUiConfig.Customize(c =>
                {
                    c.SupportHeaderParams = true;
                    c.DocExpansion = DocExpansion.Full;
                    c.SupportedSubmitMethods = new[] {HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Head};
                    c.AddOnCompleteScript(typeof (SwaggerConfig).Assembly, "Swashbuckle.TestApp.swagger_ui.ext.onComplete.js");
                });
        }
    }
}