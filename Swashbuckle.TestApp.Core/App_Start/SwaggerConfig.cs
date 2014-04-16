using System;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Application;
using Swashbuckle.TestApp.Models;
using Swashbuckle.TestApp.SwaggerExtensions;

namespace Swashbuckle.TestApp
{
    public class SwaggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            Bootstrapper.Init(config);

            SwaggerSpecConfig.Customize(c =>
                {
                    c.ResolveTargetVersionUsing((req) => "2.0");

                    c.IgnoreObsoleteActions();

                    c.PolymorphicType<Product>(pc => pc
                        .DiscriminateBy(p => p.Type)
                        .SubType<Book>()
                        .SubType<Album>()
                        .SubType<Service>(sc => sc
                            .SubType<Shipping>()
                            .SubType<Packaging>()));

                    c.OperationFilter<AddStandardResponseCodes>();
                    c.OperationFilter<AddAuthorizationResponseCodes>();

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