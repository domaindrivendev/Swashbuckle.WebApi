using System.Web.Http;
using WebActivatorEx;
using $rootnamespace$;
using Swashbuckle.Application;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace $rootnamespace$
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration 
                .EnableSwagger(c =>
                    {
                        // Use "SingleApiVersion" to describe a single version API.
                        // Swagger 2.0 requires version and title at a minimum but you can
                        // also provide additional information with the provided fluent interface
                        //
                        c.SingleApiVersion("1.0", "$rootnamespace$");

                        // If your API has multiple versions, use "MultipleApiVersions" instead of "SingleApiVersion"
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

                        // Each operation can have one or more assigned tags which may be used by consumers for various
                        // reasons. For example, the swagger-ui groups operations based on each operations first tag.
                        // By default, controller name is assigned for this but you can use the following option
                        // to override this and provide a custom value
                        //c.GroupActionsBy(apiDesc => apiDesc.HttpMethod.ToString());

                        // You can also specify a custom sort order for groups (as defined by GroupActionsBy) to dictate the
                        // order in which operations are returned. For example, if the default grouping is in place
                        // i.e. (by controller name) and you specify a descending alphabetic sort order, then actions from a
                        // ProductsController will be listed before those from a CustomersController. This would be typically
                        // used to customize the order of groupings in the swagger-ui
                        //c.OrderActionGroupsBy(new DescendingAlphabeticComparer());

                        // You can use the "BasicAuth", "ApiKey" or "OAuth2" options to define security schemes for the API
                        // See https://github.com/swagger-api/swagger-spec/blob/master/versions/2.0.md for more details
                        // NOTE: These definitions only define the schemes and need to be coupled with a corresponding "security"
                        // property at the document or operation level to indicate which schemes are required for an operation.
                        // To do this, you'll need to implement a custom IDocumentFilter and/or IOperationFilter to set these
                        // properties according to your specific authorization implementation
                        //
                        //c.BasicAuth("basic") .Description("Basic HTTP Authentication");
                        //
                        //c.ApiKey("apiKey")
                        //    .Description("API Key Authentication")
                        //    .Name("apiKey")
                        //    .In("header");
                        //
                        //c.OAuth2("oauth2")
                        //    .Description("OAuth2 Implicit Grant")
                        //    .Flow("implicit")
                        //    .AuthorizationUrl("http://petstore.swagger.wordnik.com/api/oauth/dialog")
                        //    //.TokenUrl("https://tempuri.org/token")
                        //    .Scopes(scopes =>
                        //    {
                        //        scopes.Add("read", "Read access to protected resources");
                        //        scopes.Add("write", "Write access to protected resources");
                        //    });

                        // Swashbuckle makes a best attempt at generating Swagger compliant JSON schemas for the various types
                        // exposed in your API. However, there may be occassions when more control of the output is needed.
                        // This is supported through the MapType and SchemaFilter options. The former can be used when you
                        // want to map a Type to a specific Schema (typically a primitive) rather than an auto-generated schema
                        //c.MapType<ProductType>(() => new Schema { type = "integer", format = "int32" });
                        //
                        // If you want to post-modify schema's after they've been generated, either across the board or for one
                        // specific type, you can wire up one or more schema filters. NOTE: schema filters will only be invoked
                        // for complex schema's i.e. where type = "object"
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
                .EnableSwaggerUi(c =>
                    {
                        // Use the "InjectStylesheet" option to apply one or more custom CSS stylesheets
                        // to the embedded swagger-ui that's served up by Swashbuckle
                        // NOTE: It must first be added to your project as an "Embedded Resource", then the
                        // resource's "Logical Name" can be passed to the method as shown below  
                        //
                        //c.InjectStylesheet(thisAssembly, "Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css");

                        // Set this option to allow header parameters be submitted through the swagger-ui
                        // See https://github.com/swagger-api/swagger-ui for more details
                        //
                        c.SupportHeaderParams();

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

                        // If you're API supports the OAuth2 Implicit flow, and you've described it correctly,
                        // according to the Swagger 2.0 specification (see OAuth config. above), you can
                        // enable UI support with the following command
                        //c.EnableOAuth2Support("test-client-id", "test-realm", "Swagger UI");

                        // Use the CustomAsset option to provide your own version of assets used in the swagger-ui. A typical use
                        // would be to provide your own "branded" index.html rather than the embedded version that's served up
                        // by default. As with the InjectStylesheet and InjectJavaScript options, each custom asset must be
                        // added to your project as an "Embedded Resource", and then the "Logical Name" is passed as shown below
                        //c.CustomAsset("index.html", thisAssembly, "Swashbuckle.Dummy.SwaggerExtensions.myIndex.html");
                    });
        }
    }
}