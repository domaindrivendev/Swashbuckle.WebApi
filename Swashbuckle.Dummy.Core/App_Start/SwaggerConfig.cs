using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Application;
using Swashbuckle.Dummy.Controllers;
using Swashbuckle.Dummy.SwaggerExtensions;
using System.Collections.Generic;
using System.Web.Http.Description;
using System.Web.Http.Routing.Constraints;

namespace Swashbuckle.Dummy
{
    public class SwaggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            Swashbuckle.Bootstrapper.Init(config);

            SwaggerSpecConfig.Customize(c =>
                {
                    c.IgnoreObsoleteActions();

                    //c.SupportMultipleApiVersions(GetApplicableVersionsByRouteConstraint);

                    //c.PolymorphicType<Animal>(ac => ac
                    //    .DiscriminateBy(a => a.Type)
                    //    .SubType<Kitten>());

                    c.OperationFilter<AddStandardResponseCodes>();
                    c.OperationFilter<AddAuthResponseCodes>();

                    c.IncludeXmlComments(GetXmlCommentsPath());
                });

            SwaggerUiConfig.Customize(c =>
            {
                c.SupportHeaderParams = true;
                c.DocExpansion = DocExpansion.List;
                c.SupportedSubmitMethods = new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Head };
                //c.EnableDiscoveryUrlSelector();
                //c.InjectJavaScript(typeof(SwaggerConfig).Assembly, "Swashbuckle.Dummy.SwaggerExtensions.testScript1.js");
                //c.InjectStylesheet(typeof(SwaggerConfig).Assembly, "Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css");
            });

        }

        private static string GetXmlCommentsPath()
        {
            return String.Format(@"{0}\XmlComments.xml", AppDomain.CurrentDomain.BaseDirectory);
        }

        private static IEnumerable<string> GetApplicableVersionsByRouteConstraint(ApiDescription apiDesc)
        {
            var versionConstraint = (apiDesc.Route.Constraints.ContainsKey("apiVersion"))
                ? apiDesc.Route.Constraints["apiVersion"] as RegexRouteConstraint
                : null;

            return (versionConstraint == null)
                ? new string[] {}
                : versionConstraint.Pattern.Split('|');
        }
    }
}