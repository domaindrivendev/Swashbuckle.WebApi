using Swashbuckle.Core.Application;

namespace Swashbuckle.TestApp.Core
{
    public class SwaggerConfig
    {
        public static void Customize()
        {
            SwaggerSpecConfig.Customize(c =>
                {
                    c.ResolveApiVersion((req) => "1.1");
                    c.ResolveBasePath((req) => "http://tempuri.org");
                    c.GroupDeclarationsBy((apiDesc) => "foobar");
                });


//                .ForSpec(c =>
//                    {
//                    })
//                .ForUi(c =>
//                    {
//
//                    });
//
//            SwaggerSpecConfig.Customize(c =>
//                {
//                    c.IgnoreObsoleteActions = true;
//
//                    c.SubTypesOf<Product>()
//                        .Include<Book>()
//                        .Include<Album>()
//                        .Include<Service>();
//
//                    c.SubTypesOf<Service>()
//                        .Include<Shipping>()
//                        .Include<Packaging>();
//
//                    c.PostFilter<AddStandardErrorCodes>();
//                    c.PostFilter<AddAuthorizationErrorCodes>();
//                    c.OperationFilter<ApplyCustomResponseTypes>();
//
//                    // Uncomment below to support documentation from Xml Comments
//                    // c.PostFilter(new ExtractXmlComments());
//                });

//            SwaggerSpecConfig.Customize(c =>
//                {
//                    c.IgnoreObsoleteActions = true;
//
//                    c.SubTypesOf<Product>()
//                        .Include<Book>()
//                        .Include<Album>()
//                        .Include<Service>();
//
//                    c.SubTypesOf<Service>()
//                        .Include<Shipping>()
//                        .Include<Packaging>();
//
//                    c.PostFilter<AddStandardErrorCodes>();
//                    c.PostFilter<AddAuthorizationErrorCodes>();
//                    c.OperationFilter<ApplyCustomResponseTypes>();
//
//                    // Uncomment below to support documentation from Xml Comments
//                    // c.PostFilter(new ExtractXmlComments());
//                });
//
//            SwaggerUiConfig.Customize(c =>
//                {
//                    c.SupportHeaderParams = true;
//                    c.DocExpansion = DocExpansion.List;
//                    c.SupportedSubmitMethods = new[] {HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Head};
//                    c.AddOnCompleteScript(typeof (SwaggerConfig).Assembly, "Swashbuckle.TestApp.SwaggerExtensions.onComplete.js");
//                    c.AddStylesheet(typeof(SwaggerConfig).Assembly, "Swashbuckle.TestApp.SwaggerExtensions.customStyles.css");
//                });
        }
    }
}