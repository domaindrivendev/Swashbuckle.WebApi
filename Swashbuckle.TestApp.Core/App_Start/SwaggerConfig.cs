using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Swashbuckle.Core;
using Swashbuckle.Core.Application;
using Swashbuckle.TestApp.Core.Models;
using Swashbuckle.TestApp.Core.SwaggerExtensions;

namespace Swashbuckle.TestApp.Core
{
    public class SwaggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            Bootstrapper.Init(config);

            // Capture XML comments on ApiExplorer
            var xmlCommentsPath = GetXmlCommentsPath();
            config.Services.Replace(typeof(IDocumentationProvider), new XmlCommentsDocumentationProvider(xmlCommentsPath));

            SwaggerSpecConfig.Customize(c =>
                {
                    c.ResolveApiVersion((req) => "1.0");
                    c.IgnoreObsoleteActions();

                    c.SubTypesOf<Product>()
                        .Include<Book>()
                        .Include<Album>()
                        .Include<Service>();

                    c.SubTypesOf<Service>()
                        .Include<Shipping>()
                        .Include<Packaging>();

                    c.OperationSpecFilter<AddStandardErrorCodes>();
                    c.OperationSpecFilter<AddAuthorizationErrorCodes>();
                    c.OperationSpecFilter<ExtractXmlComments>();
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