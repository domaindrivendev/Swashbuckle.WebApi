using System;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Application;
using Swashbuckle.Dummy.Models;
using Swashbuckle.Dummy.SwaggerExtensions;

namespace Swashbuckle.Dummy
{
    public class SwaggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            Swashbuckle.Bootstrapper.Init(config);

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
                    c.OperationFilter<AddAuthResponseCodes>();

                    c.IncludeXmlComments(GetXmlCommentsPath());
                });

            SwaggerUiConfig.Customize(c =>
            {
                c.SupportHeaderParams = true;
                c.DocExpansion = DocExpansion.List;
                c.SupportedSubmitMethods = new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Head };
                c.InjectJavaScript(typeof(SwaggerConfig).Assembly, "Swashbuckle.Dummy.SwaggerExtensions.testScript1.js");
                c.InjectStylesheet(typeof(SwaggerConfig).Assembly, "Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css");
            });

        }

        private static string GetXmlCommentsPath()
        {
            return String.Format(@"{0}\XmlComments.xml", AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}