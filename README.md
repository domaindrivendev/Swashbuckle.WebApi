Swashbuckle
=========

Seamlessly adds a [Swagger](http://swagger.io/) to WebApi projects! Combines ApiExplorer and Swagger/swagger-ui to provide a rich discovery, documentation and playground experience to your API consumers.

In addition to it's Swagger generator, Swashbuckle also contains an embedded version of [swagger-ui](https://github.com/swagger-api/swagger-ui) which it will automatically serve up once Swashbuckle is installed. This means you can compliment your API with a slick discovery UI to assist consumers with their integration efforts. Best of all, it requires minimal coding and maintenance, allowing you to focus on building an awesome API!

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

Swashbuckle 5.0 makes the transition to Swagger 2.0. The 2.0 schema is significantly different to it's predecessor - 1.2 and, as a result, the Swashbuckle config interface has undergone yet another overall. Checkout the [transition guide](#transitioning-to-swashbuckle-40) if you're upgrading from a prior version.

## Getting Started ##

There are currently two Nuget packages - the Core library (Swashbuckle.Core) and a convenience package (Swashbuckle) that provides automatic bootstrapping. The latter is only applicable to regular IIS hosted WepApi's. For all other hosting environments, you should just install the Core library and then follow the instructions below to manually enable the Swagger routes.

Once installed and enabled, you should be able to browse the following Swagger docs and UI endpoints:

***\<your-root-url\>/swagger/docs/1.0***

***\<your-root-url\>/swagger***

### IIS Hosted ###

If your service is hosted in IIS, you can start exposing Swagger docs and a corresponding swagger-ui by simply installing the following Nuget package:

    Install-Package Swashbuckle

This will add a reference to Swashbuckle.Core and also install a bootstrapper (App_Start/SwaggerConfig.cs) that enables the Swagger routes on app start-up using [WeActivatorEx](https://github.com/davidebbo/WebActivator).

### Self-hosted ###

If your service is self-hosted, you should just install the Core library:

    Install-Package Swashbuckle.Core

And then manually enable the Swagger docs and optionally, the swagger-ui by invoking the following extension methods (in namespace Swashbuckle.Application) on your instance of HttpConfiguration (e.g. in Program.cs)

    httpConfiguration
        .EnableSwagger(c => c.SingleApiVersion("1.0", "A title for your API"));
        .EnableSwaggerUi();

### OWIN  ###

As with Self-hosted, a WebApi served through OWIN middleware only requires the Core library

    Install-Package Swashbuckle.Core

Then manually enable the Swagger docs and swagger-ui by invoking the extension methods (in namespace Swashbuckle.Application) on your instance of HttpConfiguration (e.g. in Startup.cs)

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

Troubleshooting??? I thought this was all supposed to be "seamless"? OK you've called me out! Things shouldn't go wrong, but if they do, take a look at the [FAQ's](#faq) for inspiration.

## Configuration and Extensibility ##

The following snippet demonstrates the minimum configuration required to get the Swagger docs and swagger-ui up and running:

    httpConfiguration
        .EnableSwagger(c => c.SingleApiVersion("1.0", "A title for your API"))
        .EnableSwaggerUi();

However, these methods expose a range of configuration and extensibility options that you can pick and choose from, combining the convenience of sensible defaults with the flexibility to customize where you see fit. Read on to learn more.

### Customizing Routes ###

The default route templates for the Swagger docs and swagger-ui are "swagger/docs/{apiVersion}" and "swagger/ui/{\*assetPath}" respectively. You're free to change these so long as they include the relevant route parameters - {apiVersion} and {\*assetPath}.

    httpConfiguration
        .EnableSwagger("docs/{apiVersion}/swagger.json", c => c.SingleApiVersion("1.0", "A title for your API"))
        .EnableSwaggerUi("sandbox/{*assetPath}");

### Customizing the Generated Swagger ###

The config. interface exposes a number of methods to hook into the generation process and alter the final Swagger document. You should read through the following snippet and descriptions below to get an overview of available options. You will also find more detailed information in the [FAQ's](#faq).

    httpConfiguration
        .EnableSwagger(c => c.SingleApiVersion("1.0", "A title for your API"))
        .EnableSwagger(c =>
            {
                // By default, the service root url is inferred from the request used to access the docs.
                // However, there may be situations (e.g. certain load-balanced environments) where this does not
                // resolve correctly. You can workaround this by providing your own code to determine the root URL
                //
                c.RootUrl(req => GetRootUrlFromAppConfig());

                // Use "SingleApiVersion" to describe a single version API. Swagger 2.0 includes an "Info" object to
                // hold additional metadata for an API. Version and title are required but you may also provide the
                // additional fields with the fluent API on "SingleApiVersion"
                //
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

                // If your API has multiple versions, use "MultipleApiVersions" instead of "SingleApiVersion"
                // In this case, you must provide a lambda that tells Swashbuckle which actions should be
                // included in the docs for a given API version. Like "SingleApiVersion", each call to "Version" returns an
                // "Info" builder so you can optonally provide additional metadata per API version.
                //
                //c.MultipleApiVersions(
                //    (apiDesc, targetApiVersion) => ResolveVersionSupportByRouteConstraint(apiDesc, targetApiVersion),
                //    (vc) =>
                //    {
                //        vc.Version("2.0", "Swashbuckle Dummy API 2.0");
                //        vc.Version("1.0", "Swashbuckle Dummy API 1.0");
                //    });

                // If schemes are not explicitly provided in a Swagger 2.0 document, then the scheme used to access
                // the docs is inferred to be that of the API. If your API supports multiple schemes and you want to
                // be explicit about them, you can use the "Schemes" option as shown below.
                //
                c.Schemes(new[] { "http", "https" });

                // Each operation can have one or more assigned tags which may be used by consumers for various
                // reasons. For example, the swagger-ui groups operations based on each operations first tag.
                // By default, controller name is assigned for this but you can use the following option
                // to override this and provide a custom value
                c.GroupActionsBy(apiDesc => apiDesc.HttpMethod.ToString());

                // You can also specify a custom sort order for groups (as defined by GroupActionsBy) to dictate the
                // order in which operations are returned. For example, if the default grouping is in place
                // i.e. (by controller name) and you specify a descending alphabetic sort order, then actions from a
                // ProductsController will be listed before those from a CustomersController. This would be typically
                // used to customize the order of groupings in the swagger-ui
                c.OrderActionGroupsBy(new DescendingAlphabeticComparer());

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

                // Swashbuckle makes a best attempt at generating Swagger compliant JSON schemas for the various types
                // exposed in your API. However, there may be occassions when more control of the output is needed.
                // This is supported through the MapType and SchemaFilter options. The former can be used when you
                // want to map a Type to a specific Schema (typically a primitive) rather than an auto-generated schema
                c.MapType<ProductType>(() => new Schema { type = "integer", format = "int32" });
                //
                // If you want to post-modify schema's after they've been generated, either across the board or for one
                // specific type, you can wire up one or more schema filters. NOTE: schema filters will only be invoked
                // for complex schema's i.e. where type = "object"
                c.SchemaFilter<ApplySchemaVendorExtensions>();

                // Similar to a schema filter, Swashubuckle allows the generated Operation desrciptions to be
                // post-modified by wiring up one or more operation filters
                //
                c.OperationFilter<AddDefaultResponse>();
                //
                // If you've defined an OAuth2 flow as described above, you could use a custom filter
                // to inspect some attribute on each action and infer which (if any) OAuth2 scopes are required
                // to execute the operation
                c.OperationFilter<AssignOAuth2SecurityRequirements>();

                // Similar to a schema and operation filters, Swashubuckle allows the entire Swagger document to be
                // post-modified by wiring up one or more document filters
                //
                c.DocumentFilter<ApplyDocumentVendorExtensions>();

                // If you annotate your controllers and API types with XML comments, you can use the resulting
                // XML file (or files) to document the Operations and Schema's in the Swagger output 
                //
                c.IncludeXmlComments(GetXmlCommentsPath());

                // In contrast to WebApi, Swagger 2.0 does not include the query string component when mapping a URL
                // to an action. As a result, Swashbuckle will raise an exception if it encounters multiple actions
                // with the same path (sans query string) and HTTP method. You can workaround this by providing a
                // custom strategy to pick a winner or merge the descriptions for the purposes of the Swagger docs 
                //
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });

#### Including Xml Comments ####

If you annonate Controllers and API Types with [Xml Comments](http://msdn.microsoft.com/en-us/library/b2s063f7(v=vs.110).aspx), you can use this option to incorporate those comments into the generated spec and UI. The Xml tags are mapped to Swagger properties as follows:

* **Action summary** -> Operation.summary
* **Action remarks** -> Operation.description
* **Parameter summary** -> Parameter.description
* **Type summary** -> Schema.descripton
* **Property summary** -> Schema.description

### Customizing the swagger-ui ###

Swashbuckle supports two differrent strategies for customizing the swagger-ui

* You can provide your own version of "index.html" and customize it directly ([read about swagger-ui settings here](https://github.com/swagger-api/swagger-ui#swaggerui))
* OR, you can stick with the default, templated version and use the config. interface to tweak it accordingly

#### Custom "index.html" for swagger-ui ####

This offers more flexibilty but also forces you to maintain some HTML in your WebApi project.

If you're happy to do this, you'll need to follow these steps to make your "index.html" available to Swashbuckle:

1. Add an "index.html" to your WebApi project (or any project it has access to). You should base it off the default version [here](#).
2. Then, right click the file and open it's properties window. Change the "Build Action" to "Embedded Resource".

This will embed the file into your assembly at build-time and register it with a "Logical Name" that Swashbuckle can use to access it at runtime. The Logical Name is based on the Project's default namespace, file location and file extension . For example, if you're project's default namespace is "YourWebApiProject" and you've included the file at "/SwaggerExtensions/index.html", then you can override the default "index.html" as follows.

    httpConfiguration
        .EnableSwagger(c => c.SingleApiVersion("1.0", "A title for your API"))
        .EnableSwaggerUi(c =>
            {
                c.CustomAsset("index.html", yourAssembly, "YourWebApiProject.SwaggerExtensions.index.html");
            });
             
#### Configure the built-in "index.html" ####

If you're relatively happy with the default look and feel but would just like a few minor tweaks, you can use the default "index.html" and customize it via the config. interface. You can configure any of the standard swagger-ui settings here or inject your own stylesheets and JavaScripts to execute once the UI has loaded:

    httpConfiguration
        .EnableSwagger(c => c.SingleApiVersion("1.0", "A title for your API"))
        .EnableSwaggerUi(c =>
            {
                // Use the "InjectStylesheet" option to apply one or more custom CSS stylesheets
                // to the embedded swagger-ui that's served up by Swashbuckle
                // NOTE: It must first be added to your project as an "Embedded Resource", then the
                // resource's "Logical Name" can be passed to the method as shown below  
                //
                //c.InjectStylesheet(thisAssembly, "Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css");

                // swagger-ui renders boolean data types as a dropdown. By default it provides "true" and "false"
                // strings as the possible choices. You can use the "BooleanValues" option to change these to
                // something else.
                c.BooleanValues(new[] { "0", "1" });

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
                c.DocExpansion(DocExpansion.List);

                // If you're API has multiple versions and you've applied the "MultipleApiVersions" setting
                // as described above, you can also enable a select box that displays the corresponding discovery
                // URL's. This provides a convenient way for users to view documentation for different API versions
                //
                c.EnableDiscoveryUrlSelector();

                // If you're API supports the OAuth2 Implicit flow, and you've described it correctly,
                // according to the Swagger 2.0 specification (see OAuth config. above), you can
                // enable UI support with the following command
                c.EnableOAuth2Support("test-client-id", "test-realm", "Swagger UI");
            });

## Transitioning to Swashbuckle 5.0 ##

This version of Swashbuckle makes the transition to Swagger 2.0. The 2.0 specification is significantly different to it's predecessor - 1.2 and forces several breaking changes to Swashbuckle's config. interface. If you're using Swashbuckle without any customizations, i.e. App_Start/SwaggerConfig.cs has never been modified, then you can overwrite it with the new version. The defaults are the same and so the swagger-ui should behave as before.

\* If you have consumers of the raw Swagger document, you should ensure they can accept Swagger 2.0 before making the upgrade.

If you're using the existing config. interface to customize the final Swagger document and/or swagger-ui, you will need to port the code manually. The static __Customize__ methods on SwaggerSpecConfig and SwaggerUiConfig have been replaced with extension methods on HttpConfiguration - __EnableSwagger__ and __EnableSwaggerUi__. All options from version 4.0 are made available through these methods, albeit with slightly different naming and syntax. Refer to the tables below for a summary of changes:


| 4.0 Name/Syntax | 5.0 Equivalant | Additional Notes |
| --------------- | --------------- | ---------------- |
| ResolveBasePathUsing | RootUrl | |
| ResolveTargetVersionUsing | N/A | version is now implicit in the docs URL e.g. "swagger/docs/{apiVersion}" |
| ApiVersion | SingleApiVersion| now supports additional metadata for the version | 
| SupportMultipleApiVersions | MultipleApiVersions | now supports additional metadata for each version |
| GroupDeclarationsBy | GroupActionsBy | |
| SortDeclarationsBy | OrderActionGroupsBy | |
| Authorization | BasicAuth/ApiKey/Oauth2 | | 
| MapType | MapType | now accepts Func&lt;Schema&gt; instead of Func&lt;DataType&gt; |
| ModelFilter | SchemaFilter | IModelFilter is now ISchemaFilter, DataTypeRegistry is now SchemaRegistry |
| OperationFilter | OperationFilter | DataTypeRegistry is now SchemaRegistry |
| PolymorphicType | N/A | as of now, not supported |
| SupportHeaderParams | N/A | header params are implicitly supported |
| SupportedSubmitMethods | N/A | all HTTP verbs are implicitly supported |
| CustomRoute | CustomAsset | &nbsp; |

## Troubleshooting Steps ##

### Missing Bootststrap One-liner (*only applicable to 4.0 and above)###

As of version 4.0, Swashbuckle has no dependency on ASP.Net MVC and so, routes are no longer wired up through an MVC Area. Instead, the Swashbuckle package will install a bootstrapper (App_Start/SwaggerConfig.cs) that is invoked on app start-up using [WeActivatorEx](https://github.com/davidebbo/WebActivator). You should ensure that this file exists and is annotated with the following assembly attribute:

    [assembly: PreApplicationStartMethod(.....)]

In addition, the referenced static method should contain the following line to initiate Swashbuckle:

    Swashbuckle.Bootstrapper.Init(GlobalConfiguration.Configuration);

### IIS Hosted - UI returning 404 File not Found ###

The [swagger-ui](https://github.com/wordnik/swagger-ui) is a single page application (SPA) consisting of html, JavaScript and CSS. To serve up these files (.html, .js and .css), you're web server must execute the ASP.NET Routing Module on all requests (as opposed to just extensionless URL's). If the setting for this is not present in your Web.config, you'll need to add it manually:

    <system.webServer>
        <modules runAllManagedModulesForAllRequests="true" />
        <!-- Other web server settings -->
    </system.webServer>
    
### OWIN Hosted in IIS - UI returning 404 File not Found

This is similar to the issue above with an additonal workaround required. To ensure that the OWIN module is run for all requests (extension and extensionless), **runAllManagedModulesForAllRequests** must be set in the Web.config.

In addition, a stage marker must be used in Startup.cs, AFTER configuring the WebApi middleware, to ensure that routes with extensions are also processed via WebApi:

    app.UseWebApi(config);
    app.UseStageMarker(PipelineStage.MapHandler);

### Conflicting Model Id's ###

If you see the following error message in the Swagger UI ...

***Unable to read api '.....' from path ..... (server returned undefined)***

It's likely because something went wrong during the spec generation. You can dig a little deeper by browsing to the path in question. When you do this, you may see the following error message:

***Failed to generate Swagger models with unique Id's. Do you have multiple API types with the same class name?***

This is by design and will occur if one or more of your API types have conflicting class names - e.g. Namespace1.Customer, Namespace2.Customer etc. Actually, the class names need only be unique within a given ApiDeclaration, the scope if which is customizeable via the [GroupDeclarationsBy](#groupdeclarationsby) option described below. 

### Issues with VS 2013 ###

VS 2103 ships with a new feature - Browser Link that improves the web development workflow by setting up a channel between the IDE and pages being previewed in a local browser. It does this by dynamically injecting JavaScript into your files.

Although this JavaScript SHOULD have no affect on your production code, it appears to be breaking the swagger-ui.

I hope to find a permanent fix but in the meantime, you'll need to workaround this isuse by disabling the feature in your web.config:

    <appSettings>
        <add key="vs:EnableBrowserLink" value="false"/>
    </appSettings>< appSettings> 

### Missing Area Registration (*only applicable to 3.x and below)###

Prior to version 4.0, Swashbuckle wires up it's routes as an MVC Area. In MVC projects, all Areas are usually registered at application startup. If the code to do this is not present in your Global.asax.cs, you'll need to add it manually:

    protected void Application_Start()
    {
        // Other boot-strapping ...
        
        AreaRegistration.RegisterAllAreas();
    }