using System;
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
                    c.ResolveBasePathUsing((req) => "1.0");

                    c.IgnoreObsoleteActions();

                    c.OperationFilter<AddStandardErrorCodes>();
                    c.OperationFilter<AddAuthorizationErrorCodes>();

                    c.PolymorphicType<Product>(pc => pc
                        .DiscriminateBy(p => p.Type)
                        .SubType<Book>()
                        .SubType<Album>()
                        .SubType<Service>(sc => sc
                            .SubType<Shipping>()
                            .SubType<Packaging>()));

                    c.IncludeXmlComments(GetXmlCommentsPath());
                });

            SwaggerUiConfig.Customize(c =>
                {
                    c.SupportHeaderParams = true;
                    c.DocExpansion = DocExpansion.List;
                    c.SupportedSubmitMethods = new[] {HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Head};
                    c.InjectJavaScript(typeof (SwaggerConfig).Assembly, "Swashbuckle.TestApp.Core.SwaggerExtensions.customScript.js");
                    c.InjectStylesheet(typeof (SwaggerConfig).Assembly, "Swashbuckle.TestApp.Core.SwaggerExtensions.customStyles.css");
                });
        }

        private static string GetXmlCommentsPath()
        {
            return String.Format(@"{0}XmlComments.xml", AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}