using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Core.Application;
using Swashbuckle.TestApp.Core.Models;
using Swashbuckle.TestApp.Core.SwaggerExtensions;

namespace Swashbuckle.TestApp.Core
{
    public class SwaggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            Swashbuckle.Core.Bootstrapper.Init(config);

            SwaggerSpecConfig.Customize(c =>
                {
                    c.ResolveApiVersion((req) => "1.0");
                    c.IgnoreObsoleteActions();
                    c.GroupDeclarationsBy((apiDesc) => "foobar");

                    c.SubTypesOf<Product>()
                        .Include<Book>()
                        .Include<Album>()
                        .Include<Service>();
                    
                    c.SubTypesOf<Service>()
                        .Include<Shipping>()
                        .Include<Packaging>();
                    
                    c.OperationSpecFilter<AddStandardErrorCodes>();
                    c.OperationSpecFilter<AddAuthorizationErrorCodes>();
                    
                    // Uncomment below to support documentation from Xml Comments
                    // c.PostFilter(new ExtractXmlComments());
                });

            SwaggerUiConfig.Customize(c =>
                {
                    c.SupportHeaderParams = true;
                    c.DocExpansion = DocExpansion.List;
                    c.SupportedSubmitMethods = new[] {HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Head};
                    c.InjectJavaScript(typeof (SwaggerConfig).Assembly, "Swashbuckle.TestApp.Core.SwaggerExtensions.customScript.js");
                    c.InjectStylesheet(typeof(SwaggerConfig).Assembly, "Swashbuckle.TestApp.Core.SwaggerExtensions.customStyles.css");
                });
        }
    }
}