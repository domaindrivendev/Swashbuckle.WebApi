using System.Web.Http;
using $rootnamespace$;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace $rootnamespace$
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            Swashbuckle.Configuration.Instance
                .SwaggerDocs(c =>
                    {
                        // Use "SingleApiVersion" to describe a single version API
                        // Swagger 2.0 requires version and title at a minimum but you can
                        // also provide additional information
                        //
                        c.SingleApiVersion("1.0", "$rootnamespace$ API");

                        // If you API has multiple versions, use "MultipleApiVersions" instead of "SingleApiVersion"
                        // In this case, you must provide a lambda that tells Swashbuckle which actions should be
                        // included in the docs for a given API version
                        //
                        //c.MultipleApiVersions(
                        //    (apiDesc, targetApiVersion) => ResolveVersionSupportByRouteConstraint(apiDesc, targetApiVersion),
                        //    (vc) =>
                        //    {
                        //        vc.Version("1.0", "Swashbuckle Dummy API 1.0");
                        //        vc.Version("2.0", "Swashbuckle Dummy API 2.0");
                        //    });

                        // If schemes are not specifically provided in a Swagger 2.0 document, then the scheme used to access
                        // the docs is inferred to be that of the API. If your API supports multiple schemes and you want to
                        // be explicit about them, you can use the "Schemes" option as shown below.
                        //
                        //c.Schemes(new[] { "http", "https" });

                        // You can use the "BasicAuth", "ApiKey" or "OAuth2" options to define security schemes for the API
                        // See https://github.com/swagger-api/swagger-spec/blob/master/versions/2.0.md for more details
                        // NOTE: These definitions only define the schemes and need to be coupled with a corresponding "security"
                        // property at the document or operation level to indicate which schemes are required for an operation.
                        // To do this, you'll need to implement a custom IDocumentFilter and/or IOperationFilter to set these
                        // properties according to your specific authorization implementation
                        //
                        //c.BasicAuth("basic")
                        //    .Description("Basic HTTP Authentication");
                        //
                        //c.ApiKey("apiKey")
                        //    .Description("API Key Authentication")
                        //    .Name("apiKey")
                        //    .In("header");
                        //
                        //c.OAuth2("oauth2")
                        //    .Description("OAuth2 Authorization Code Grant")
                        //    .Flow("implicit")
                        //    .AuthorizationUrl("https://tempuri.org/auth")
                        //    //.TokenUrl("https://tempuri.org/token")
                        //    .Scopes(s =>
                        //    {
                        //        s.Add("read", "Read access to protected resources");
                        //        s.Add("write", "Write access to protected resources");
                        //    });

                        // Swashbuckle makes a best attempt at generating Swagger compliant JSON schemas for the various types
                        // exposed in your API. However, there may be occassions when more control of the output is required
                        // In this case, you can post-modify each of the generated schemas by wiring up one or more schema filters
                        //
                        //c.SchemaFilter<ApplySchemaVendorExtensions>();

                        // Similar to a schema filter, Swashubuckle allows the generated Operation desrciptions to be
                        // post-modified by wiring up one or more operation filters
                        //
                        //c.OperationFilter<AddDefaultResponse>();
                        //
                        // If you've defined an OAuth2 flow as described above, you could use a custom filter
                        // to inspect some attribute on each action and infer which (if any) OAuth2 scopes are required
                        // to execute the operation
                        //c.OperationFilter<AssignOAuth2SecurityRequirements>();

                        // Similar to a schema and operation filters, Swashubuckle allows the entire Swagger document to be
                        // post-modified by wiring up one or more document filters
                        //
                        //c.DocumentFilter<ApplyDocumentVendorExtensions>();

                        // If you annotate your controllers and API types with XML comments, you can use the resulting
                        // XML file (or files) to document the Operations and Schema's in the Swagger output 
                        //
                        //c.IncludeXmlComments(GetXmlCommentsPath());

                        // In contrast to WebApi, Swagger 2.0 does not include the query string component when mapping a URL
                        // to an action. As a result, Swashbuckle will raise an exception if it encounters multiple actions
                        // with the same path (sans query string) and HTTP method. You can workaround this by providing a
                        // custom strategy to pick a winner or merge the descriptions for the purposes of the Swagger docs 
                        //
                        //c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                    })
                .SwaggerUi(c =>
                    {
                        // If you're only using Swashbuckle to expose the raw swagger docs, you can use
                        // the "Disable" option to completely disable routes to the embedded swagger-ui
                        //
                        //c.Disable();

                        // Use the "InjectStylesheet" option to apply one or more custom CSS stylesheets
                        // to the embedded swagger-ui that's served up by Swashbuckle
                        // NOTE: It must first be added to your project as an "Embedded Resource", then the
                        // resource's "Logical Name" can be passed to the method as shown below  
                        //
                        //c.InjectStylesheet(thisAssembly, "Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css");

                        // Set this option to allow header parameters be submitted through the swagger-ui
                        // See https://github.com/swagger-api/swagger-ui for more details
                        //
                        //c.SupportHeaderParams();

                        // Specify which HTTP methods should be supported through the swagger-ui
                        // See https://github.com/swagger-api/swagger-ui for more details
                        //
                        //c.SupportedSubmitMethods(new[] { HttpMethod.Post, HttpMethod.Get, HttpMethod.Put, HttpMethod.Delete });

                        // Use the "InjectJavaScript" option to invoke one or more custom Javascripts
                        // after the swagger-ui has loaded
                        // NOTE: It must first be added to your project as an "Embedded Resource", then the
                        // resource's "Logical Name" can be passed to the method as shown below  
                        //
                        //c.InjectJavaScript(thisAssembly, "Swashbuckle.Dummy.SwaggerExtensions.testScript1.js");

                        // Specify the default expansion for sections in the swagger-ui when it initially loads
                        // Possible values are "None", "List" and "Full"
                        // See https://github.com/swagger-api/swagger-ui for more details
                        //
                        //c.DocExpansion(DocExpansion.List);

                        // If you're API has multiple versions and you've applied the "MultipleApiVersions" setting
                        // as described above, you can also enable a select box that displays the corresponding discovery
                        // URL's. This provides a convenient way for users to view documentation for different API versions
                        //
                        //c.EnableDiscoveryUrlSelector();
                    })
                .Init(GlobalConfiguration.Configuration);
        }
    }
}