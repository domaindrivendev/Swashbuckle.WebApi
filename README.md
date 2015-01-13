Swashbuckle 5.0 Alpha
=========

Seamlessly adds a [Swagger](http://swagger.io/) to WebApi projects! Combines ApiExplorer and Swagger/swagger-ui to provide a rich discovery, documentation and playground experience to your API consumers.

In addition to its Swagger generator, Swashbuckle also contains an embedded version of [swagger-ui](https://github.com/swagger-api/swagger-ui) which it will automatically serve up once Swashbuckle is installed. This means you can compliment your API with a slick discovery UI to assist consumers with their integration efforts. Best of all, it requires minimal coding and maintenance, allowing you to focus on building an awesome API!

And that's not all ...

Once you have a Web API that can describe itself in Swagger, you've opened the treasure chest of Swagger-based tools including a client generator that can be targetted to a wide range of popular platforms. See [swagger-codegen](https://github.com/swagger-api/swagger-codegen) for more details.

**Swashbuckle Core Features:**

* Auto-generated [Swagger 2.0](https://github.com/swagger-api/swagger-spec/blob/master/versions/2.0.md)
* Seamless integration of swagger-ui
* Reflection-based Schema generation for describing API types
* Extensibility hooks for customizing the generated Swagger doc
* Extensibility hooks for customizing the swagger-ui
* Out-of-the-box support for leveraging Xml comments
* Support for describing ApiKey, Basic Auth and OAuth2 schemes ... including UI support for the Implicit OAuth2 flow

**\*Swashbuckle 5.0**

Swashbuckle 5.0 makes the transition to Swagger 2.0. The 2.0 schema is significantly different to its predecessor - 1.2 and, as a result, the Swashbuckle config interface has undergone yet another overhaul. Checkout the [transition guide](#transitioning-to-swashbuckle-50) if you're upgrading from a prior version.

## Getting Started ##

There are currently two Nuget packages - the Core library (Swashbuckle.Core) and a convenience package (Swashbuckle) that provides automatic bootstrapping. The latter is only applicable to regular IIS hosted WepApi's. For all other hosting environments, you should only install the Core library and then follow the instructions below to manually enable the Swagger routes.

Once installed and enabled, you should be able to browse the following Swagger docs and UI endpoints respectively:

***\<your-root-url\>/swagger/docs/1.0***

***\<your-root-url\>/swagger***

### IIS Hosted ###

If your service is hosted in IIS, you can start exposing Swagger docs and a corresponding swagger-ui by simply installing the following Nuget package:

    Install-Package Swashbuckle -Pre

This will add a reference to Swashbuckle.Core and also install a bootstrapper (App_Start/SwaggerConfig.cs) that enables the Swagger routes on app start-up using [WeActivatorEx](https://github.com/davidebbo/WebActivator).

### Self-hosted ###

If your service is self-hosted, just install the Core library:

    Install-Package Swashbuckle.Core -Pre

And then manually enable the Swagger docs and optionally, the swagger-ui by invoking the following extension methods (in namespace Swashbuckle.Application) on an instance of HttpConfiguration (e.g. in Program.cs)

    httpConfiguration
        .EnableSwagger(c => c.SingleApiVersion("1.0", "A title for your API"));
        .EnableSwaggerUi();

### OWIN  ###

If your service is hosted using OWIN middleware, just install the Core library:

    Install-Package Swashbuckle.Core -Pre

Then manually enable the Swagger docs and swagger-ui by invoking the extension methods (in namespace Swashbuckle.Application) on an instance of HttpConfiguration (e.g. in Startup.cs)

    httpConfiguration
        .EnableSwagger(c => c.SingleApiVersion("1.0", "A title for your API"));
        .EnableSwaggerUi();

\* If your OWIN middleware is self-hosted then your done! If your using OWIN through the IIS Integrated pipeline then you'll need to apply the following steps to prevent URL's with extensions (i.e. the swagger-ui assets) from being short-circuited by the native static file module.

1) In your web.config add:

    <configuration>
       <system.webServer>
          <modules runAllManagedModulesForAllRequests=“true” />
       </system.webServer>
    </configuration>

2) Add the following stage marker AFTER configuring the WebApi middleware (in namespace Microsoft.Owin.Extensions):

    app.UseStageMarker(PipelineStage.MapHandler);
    
This setting causes the WebApi middleware to execute earlier in the pipeline, allowing it to correctly handle URL's with extensions.

Check out the following articles for more information:

<https://katanaproject.codeplex.com/wikipage?title=Static%20Files%20on%20IIS>

<http://www.asp.net/aspnet/overview/owin-and-katana/owin-middleware-in-the-iis-integrated-pipeline>

## Troubleshooting ##

Troubleshooting??? I thought this was all supposed to be "seamless"? OK you've called me out! Things shouldn't go wrong, but if they do, take a look at the [FAQ's](#troubleshooting-and-faqs) for inspiration.

## Customizing the Generated Swagger Docs ##

The following snippet demonstrates the minimum configuration required to get the Swagger docs and swagger-ui up and running:

    httpConfiguration
        .EnableSwagger(c => c.SingleApiVersion("1.0", "A title for your API"))
        .EnableSwaggerUi();

These methods expose a range of configuration and extensibility options that you can pick and choose from, combining the convenience of sensible defaults with the flexibility to customize where you see fit. Read on to learn more.

### Custom Routes ###

The default route templates for the Swagger docs and swagger-ui are "swagger/docs/{apiVersion}" and "swagger/ui/{\*assetPath}" respectively. You're free to change these so long as the provided templates include the relevant route parameters - {apiVersion} and {\*assetPath}.

    httpConfiguration
        .EnableSwagger("docs/{apiVersion}/swagger.json", c => c.SingleApiVersion("1.0", "A title for your API"))
        .EnableSwaggerUi("sandbox/{*assetPath}");

### Additional Service Metadata ###

In addition to operation descriptions, Swagger 2.0 includes several properties to describe the service itself. These can all be provided through the config. interface:

    httpConfiguration
        .EnableSwagger(c =>
            {
                c.RootUrl(req => GetRootUrlFromAppConfig());

                c.Schemes(new[] { "http", "https" });

                c.SingleApiVersion("1.0", "Swashbuckle Dummy")
                    .Description("A sample API for testing and prototyping Swashbuckle features")
                    .TermsOfService("Some terms")
                    .Contact(cc => cc
                        .Name("Some contact")
                        .Url("http://tempuri.org/contact")
                        .Email("some.contact@tempuri.org"))
                    .License(lc => lc
                        .Name("Some License")
                        .Url("http://tempuri.org/license"));

            });

#### RootUrl ####

By default, the service root url is inferred from the request used to access the docs. However, there may be situations (e.g. proxy and load-balanced environments) where this does not resolve correctly. You can workaround this by providing your own code to determine the root URL.

#### Schemes ####

If schemes are not explicitly provided in a Swagger 2.0 document, then the scheme used to access the docs is taken as the default. If your API supports multiple schemes and you want to be explicit about them, you can use the __Schemes__ option.

#### SingleApiVersion ####

Use this to describe a single version API. Swagger 2.0 includes an "Info" object to hold additional metadata for an API. Version and title are required but you may also provide additional fields as shown above.

### Describing Multiple API Versions ###

If your API has multiple versions, use __MultipleApiVersions__ instead of __SingleApiVersion__. In this case, you provide a lambda that tells Swashbuckle which actions should be included in the docs for a given API version. Like __SingleApiVersion__, __Version__ also returns an "Info" builder so you can provide additional metadata per API version.

    httpConfiguration
        .EnableSwagger(c =>
            {
                c.MultipleApiVersions(
                    (apiDesc, targetApiVersion) => ResolveVersionSupportByRouteConstraint(apiDesc, targetApiVersion),
                    (vc) =>
                    {
                        vc.Version("2.0", "Swashbuckle Dummy API 2.0");
                        vc.Version("1.0", "Swashbuckle Dummy API 1.0");
                    });
            });
        .EnableSwaggerUi(c =>
            {
                c.EnableDiscoveryUrlSelector();
            });

\* You can also enable a select box in the swagger-ui (as shown above) that displays a discovery URL for each version. This provides a convenient way for users to browse documentation for different API versions.

### Describing Security/Authorization Schemes ###

You can use BasicAuth, __ApiKey__ or __OAuth2__ options to describe security schemes for the API. See https://github.com/swagger-api/swagger-spec/blob/master/versions/2.0.md for more details.

    httpConfiguration
        .EnableSwagger(c =>
            {
                //c.BasicAuth("basic")
                //    .Description("Basic HTTP Authentication");

                //c.ApiKey("apiKey")
                //    .Description("API Key Authentication")
                //    .Name("apiKey")
                //    .In("header");

                c.OAuth2("oauth2")
                    .Description("OAuth2 Implicit Grant")
                    .Flow("implicit")
                    .AuthorizationUrl("http://petstore.swagger.wordnik.com/api/oauth/dialog")
                    //.TokenUrl("https://tempuri.org/token")
                    .Scopes(scopes =>
                    {
                        scopes.Add("read", "Read access to protected resources");
                        scopes.Add("write", "Write access to protected resources");
                    });

                c.OperationFilter<AssignOAuth2SecurityRequirements>();
            });
        .EnableSwaggerUi(c =>
            {
                c.EnableOAuth2Support("test-client-id", "test-realm", "Swagger UI");
            });

__NOTE:__ These only define the schemes and need to be coupled with a corresponding "security" property at the document or operation level to indicate which schemes are required for each operation.  To do this, you'll need to implement a custom IDocumentFilter and/or IOperationFilter to set these properties according to your specific authorization implementation

\* If you're API supports the OAuth2 Implicit flow, and you've described it correctly, according to the Swagger 2.0 specification, you can enable UI support as shown above.

### Customize the Operation Listing ###

If necessary, you can provide custom strategies to group and sort the list of Operations in a Swagger document:

    httpConfiguration
        .EnableSwagger(c =>
            {
                c.GroupActionsBy(apiDesc => apiDesc.HttpMethod.ToString());

                c.OrderActionGroupsBy(new DescendingAlphabeticComparer());
            });

#### GroupActionsBy ####

Each operation can be assigned one or more tags which are then used by consumers for various reasons. For example, the swagger-ui groups operations according to the first tag of each operation. By default, this will be controller name but you can use this method to override with any value.

#### OrderActionGroupsBy ####

You can also specify a custom sort order for groups (as defined by __GroupActionsBy__) to dictate the order in which operations are listed. For example, if the default grouping is in place (controller name) and you specify a descending alphabetic sort order, then actions from a ProductsController will be listed before those from a CustomersController. This is typically used to customize the order of groupings in the swagger-ui.

### Modifying Generated Schemas ###

Swashbuckle makes a best attempt at generating Swagger compliant JSON schemas for the various types exposed in your API. However, there may be occassions when more control of the output is needed.  This is supported through the following options:

    httpConfiguration
        .EnableSwagger(c =>
            {
                c.MapType<ProductType>(() => new Schema { type = "integer", format = "int32" });

                c.SchemaFilter<ApplySchemaVendorExtensions>();

                c.UseFullTypeNameInSchemaIds();

                c.DescribeAllEnumsAsStrings();
            });

#### MapType ####

Use this option to override the Schema generation for a specific type.

It should be noted that the resulting Schema will be placed "inline" for any applicable Operations. While Swagger 2.0 supports inline definitions for "all" Schema types, the swagger-ui tool does not. It expects "complex" Schemas to be defined separately and referenced. For this reason, you should only use the __MapType__ option when the resulting Schema is a primitive or array type.

If you need to alter a complex Schema, use a Schema filter.

#### SchemaFilter ####

If you want to post-modify "complex" Schemas once they've been generated, across the board or for a specific type, you can wire up one or more Schema filters.

ISchemaFilter has the following interface:

    void Apply(Schema schema, schemaRegistry schemaRegistry, Type type);

A typical implementation will inspect the system Type and modify the Schema accordingly. If necessary, the schemaRegistry can be used to obtain or register Schemas for other Types

#### UseFullTypeNamesInSchemaIds ####

In a Swagger 2.0 document, complex types are typically declared globally and referenced by unique Schema Id. By default, Swashbuckle does NOT use the full type name in Schema Ids. In most cases, this works well because it prevents the "implementation detail" of type namespaces from leaking into your Swagger docs and UI. However, if you have multiple types in your API with the same class name, you'll need to opt out of this behavior to avoid Schema Id conflicts.  

#### DescribeAllEnumsAsStrings ####

In accordance with the built in JsonSerializer, Swashbuckle will, by default, describe enums as integers. You can change the serializer behavior by configuring the StringToEnumConverter globally or for a given enum type. Swashbuckle will honor this change out-of-the-box. However, if you use a different approach to serialize enums as strings, you can also force Swashbuckle to describe them as strings.

### Modifying Generated Operations ###

Similar to Schema filters, Swashbuckle also supports Operation and Document filters:

    httpConfiguration
        .EnableSwagger(c => c.SingleApiVersion("1.0", "A title for your API"))
            {
                c.OperationFilter<AddDefaultResponse>();

                c.DocumentFilter<ApplyDocumentVendorExtensions>();
            });

#### OperationFilter ####

Post-modify Operation descriptions once they've been generated by wiring up one or more Operation filters.

IOperationFilter has the following interface:

    void Apply(Operation operation, schemaRegistry schemaRegistry, ApiDescription apiDescription);

A typical implementation will inspect the ApiDescription and modify the Operation accordingly. If necessary, the schemaRegistry can be used to obtain or register Schemas for Types that are used in the Operation.

#### DocumentFilter ####

Post-modify the entire Swagger document by wiring up one or more Document filters.

IDocumentFilter has the following interface:

    void Apply(SwaggerDocument swaggerDoc, schemaRegistry schemaRegistry, IApiExplorer apiExplorer);

This gives full control to modify the final SwaggerDocument. You can gain additional context from the provided SwaggerDocument (e.g. version) and IApiExplorer. You should have a good understanding of the [Swagger 2.0 spec.](https://github.com/swagger-api/swagger-spec/blob/master/versions/2.0.md) before using this option.

### Including XML Comments ###

If you annonate Controllers and API Types with [Xml Comments](http://msdn.microsoft.com/en-us/library/b2s063f7(v=vs.110).aspx), you can incorporate those comments into the generated docs and UI. The Xml tags are mapped to Swagger properties as follows:

* **Action summary** -> Operation.summary
* **Action remarks** -> Operation.description
* **Parameter summary** -> Parameter.description
* **Type summary** -> Schema.descripton
* **Property summary** -> Schema.description (i.e. on a property Schema)

You can enable this by providing the path to one or more XML comments files:

    httpConfiguration
        .EnableSwagger(c => c.SingleApiVersion("1.0", "A title for your API"))
            {
                c.IncludeXmlComments(GetXmlCommentsPathForControllers());
                c.IncludeXmlComments(GetXmlCommentsPathForModels());
            });

#### Response Codes ####

Swashbuckle will automatically create a "success" response for each operation based on the action's return type. If it's a void, the status code will be 204 (No content), otherwise 200 (Ok). This mirrors WebApi's default behavior. If you need to change this and/or list additional response codes, you can use the non-standard "response" tag:

    /// <response code="201">Account created</response>
    /// <response code="400">Username already in use</response>
    public int Create(Account account)

### Working Around Swagger 2.0 Constraints ###

In contrast to WebApi, Swagger 2.0 does not include the query string component when mapping a URL to an action. As a result, Swashbuckle will raise an exception if it encounters multiple actions with the same path (sans query string) and HTTP method. You can workaround this by providing a custom strategy to pick a winner or merge the descriptions for the purposes of the Swagger docs 

    httpConfiguration
        .EnableSwagger(c => c.SingleApiVersion("1.0", "A title for your API"))
            {
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });

See the following discusssion for more details:

<https://github.com/domaindrivendev/Swashbuckle/issues/142>

## Customizing the swagger-ui ##

The swagger-ui is a JavaScript application hosted in a single HTML page (index.html), and it exposes several customization settings. Swashbuckle ships with an embedded version and includes corresponding config. methods for each of the UI settings. If you require further customization, you can also inject your own version of "index.html". Read on to learn more.

### Customizations via Config. Interface ###

If your happy with the basic look and feel but want to make some minor tweaks, the following options may be sufficient:

    httpConfiguration
        .EnableSwagger(c => c.SingleApiVersion("1.0", "A title for your API")) co
        .EnableSwaggerUi(c =>
            {
                c.InjectStylesheet(containingAssembly, "Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css");

                c.InjectJavaScript(containingAssembly, "Swashbuckle.Dummy.SwaggerExtensions.testScript1.js");

                c.BooleanValues(new[] { "0", "1" });

                c.DocExpansion(DocExpansion.List);
            });

#### InjectStylesheet ####

Use this to enrich the UI with one or more additional CSS stylesheets. The file must be included in your project as an "Embedded Resource", and then the resource's "Logical Name" is passed to the method as shown above. See [Injecting Custom Content](#injecting-custom-content) for step by step instructions.

#### InjectJavaScript ####

Use this to invoke one or more custom JavaScripts after the swagger-ui has loaded. The file must be included in your project as an "Embedded Resource", and then the resource's "Logical Name" is passed to the method as shown above. See [Injecting Custom Content](#injecting-custom-content) for step by step instructions.

#### BooleanValues ####

The swagger-ui renders boolean data types as a dropdown. By default, it provides "true" and "false" strings as the possible choices. You can use this option to change these to something else, for example 0 and 1.

#### DocExpansion ####

Use this option to control how the Operation listing is displayed. It can be set to "None" (default), "List" (shows operations for each resource), or "Full" (fully expanded: shows operations and their details).

### Provide your own "index.html" ###

As an alternative, you can inject your own version of "index.html" and customize the markup and swagger-ui directly. Use the __CustomAsset__ option to instruct Swashbuckle to return your version instead of the default when a request is made for "index.html". As with all custom content, the file must be included in your project as an "Embedded Resource", and then the resource's "Logical Name" is passed to the method as shown below. See [Injecting Custom Content](#injecting-custom-content) for step by step instructions.

For compatibility, you should base your custom "index.html" off [this version](https://github.com/swagger-api/swagger-ui/blob/8831e289c0e2dc297754e92b6848c386a88ab37a/dist/index.html)

    httpConfiguration
        .EnableSwagger(c => c.SingleApiVersion("1.0", "A title for your API"))
        .EnableSwaggerUi(c =>
            {
                c.CustomAsset("index.html", yourAssembly, "YourWebApiProject.SwaggerExtensions.index.html");
            });

### Injecting Custom Content ###

The __InjectStylesheet__, __InjectJavaScript__ and __CustomAsset__ options all share the same mechanism for providing custom content. In each case, the file must be included in your project as an "Embedded Resource". The steps to do this are described below:

1. Add a new file to your WebApi project.
2. In Solution Explorer, right click the file and open its properties window. Change the "Build Action" to "Embedded Resource".

This will embed the file in your assembly and register it with a "Logical Name". This can then be passed to the relevant config. method. It's based on the Project's default namespace, file location and file extension. For example, given a default namespace of "YourWebApiProject" and a file located at "/SwaggerExtensions/index.html", then the resource will be assigned the name - "YourWebApiProject.SwaggerExtensions.index.html".

## Transitioning to Swashbuckle 5.0 ##

This version of Swashbuckle makes the transition to Swagger 2.0. The 2.0 specification is significantly different to its predecessor - 1.2 and forces several breaking changes to Swashbuckle's config. interface. If you're using Swashbuckle without any customizations, i.e. App_Start/SwaggerConfig.cs has never been modified, then you can overwrite it with the new version. The defaults are the same and so the swagger-ui should behave as before.

\* If you have consumers of the raw Swagger document, you should ensure they can accept Swagger 2.0 before making the upgrade.

If you're using the existing config. interface to customize the final Swagger document and/or swagger-ui, you will need to port the code manually. The static __Customize__ methods on SwaggerSpecConfig and SwaggerUiConfig have been replaced with extension methods on HttpConfiguration - __EnableSwagger__ and __EnableSwaggerUi__. All options from version 4.0 are made available through these methods, albeit with slightly different naming and syntax. Refer to the tables below for a summary of changes:


| 4.0 | 5.0 Equivalant | Additional Notes |
| --------------- | --------------- | ---------------- |
| ResolveBasePathUsing | RootUrl | |
| ResolveTargetVersionUsing | N/A | version is now implicit in the docs URL e.g. "swagger/docs/{apiVersion}" |
| ApiVersion | SingleApiVersion| now supports additional metadata for the version | 
| SupportMultipleApiVersions | MultipleApiVersions | now supports additional metadata for each version |
| Authorization | BasicAuth/ApiKey/OAuth2 | | 
| GroupDeclarationsBy | GroupActionsBy | |
| SortDeclarationsBy | OrderActionGroupsBy | |
| MapType | MapType | now accepts Func&lt;Schema&gt; instead of Func&lt;DataType&gt; |
| ModelFilter | SchemaFilter | IModelFilter is now ISchemaFilter, DataTypeRegistry is now schemaRegistry |
| OperationFilter | OperationFilter | DataTypeRegistry is now schemaRegistry |
| PolymorphicType | N/A | not currently supported |
| SupportHeaderParams | N/A | header params are implicitly supported |
| SupportedSubmitMethods | N/A | all HTTP verbs are implicitly supported |
| CustomRoute | CustomAsset | &nbsp; |

## Troubleshooting and FAQ's ##

1. [Page not found when accessing the UI](#page-not-found-when-accessing-ui)
2. [swagger-ui broken by Visual Studio 2013](#swagger-ui-broken-by-visual-studio-2013)
3. [How to add vendor extensions](#how-to-add-vendor-extensions)
4. [How to describe multiple API versions](#how-to-describe-multiple-api-versions)
5. [How to configure OAuth2 support](#how-to-configure-oauth2-support)

### Page not found when accessing the UI ###

This issue ocurrs in IIS hosted environments when the native "static file" module is setup to intercept requests for URL's that contain extensions, bypassing the WebApi pipeline. In Swashbuckle, all of the swagger-ui assets (.html, .js, .css etc.) are served through the WebApi pipeline and so the static file handler must be disabled for these assets. In your web.config, add the following:

    <system.webServer>
    <modules>
      <remove name="UrlRoutingModule-4.0" />
      <add name="UrlRoutingModule-4.0" type="System.Web.Routing.UrlRoutingModule" preCondition="" />
    </modules>
    </system.webServer>

### Swagger-ui broken by Visual Studio 2013 ###

VS 2103 ships with a new feature - Browser Link that improves the web development workflow by setting up a channel between the IDE and pages being previewed in a local browser. It does this by dynamically injecting JavaScript into your files.

Although this JavaScript SHOULD have no affect on your production code, it appears to be breaking the swagger-ui.

I hope to find a permanent fix but in the meantime, you'll need to workaround this isuse by disabling the feature in your web.config:

    <appSettings>
        <add key="vs:EnableBrowserLink" value="false"/>
    </appSettings>< appSettings>

### How to add vendor extensions ###

Swagger 2.0 allows additional meta-data (aka vendor extensions) to be added at various points in the Swagger document. Swashbuckle supports this by including a "vendorExtensions" dictionary with each of the extensible Swagger types. Meta-data can be added to these dictionaries from custom Schema, Operation or Document filters. For example:

    public class ApplySchemaVendorExtensions : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            schema.vendorExtensions.Add("x-foo", "bar");
        }
    }

As per the specification, all extension properties should be prefixed by "x-" 

### How to describe multiple API versions ###

TODO

### How to confiugre OAuth2 support

TODO