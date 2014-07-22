Swashbuckle
=========

Seamlessly adds a [swagger](https://developers.helloreverb.com/swagger) to WebApi projects! Combines ApiExplorer and swagger/swagger-ui to provide a rich discovery and documentation experience to your API consumers.

In addition to it's Swagger generator, Swashbuckle also contains an embedded version of [swagger-ui](https://github.com/wordnik/swagger-ui.git) which it will automatically serve up once Swashbuckle is installed. This means little or no maintenance for discovery and documentation of your service, allowing you to focus on building an awesome API!

And that's not all ...

Once you have a Web API that can describe itself in Swagger, you've opened the treasure chest of Swagger-based tools including a client generator that can be targetted to a wide range of popular platforms. See [swagger-codegen](https://github.com/wordnik/swagger-codegen) for more details.

**Swashbuckle Core Features:**

* Auto-generated [Swagger 1.2](https://github.com/wordnik/swagger-spec/blob/master/versions/1.2.md)
* Seamless integration of swagger-ui
* Reflection-based type/model descriptions ... including support for polymorphic types
* Extensibility hooks for customizing the generated spec
* Extensibility hooks for customizing the swagger-ui
* Out-of-the-box support for leveraging Xml comments

**\*Swashbuckle 4.0**

As of version 4.0, Swashbuckle has no dependency on ASP.Net MVC. As a result, it's now available to IIS hosted, self-hosted and OWIN-hosted Web API's. However, this introduces several (relatively) trivial breaking changes. Checkout the [transition guide](#transitioning-to-swashbuckle-40) if you're upgrading from a prior version.

## Getting Started ##

### IIS Hosted ###

If youre service is hosted in IIS, you can start exposing Swagger docs and a corresponding Swagger UI by simply installing the following Nuget package:

    Install-Package Swashbuckle

This will add a reference to Swashbuckle.Core, which contains the generator and embedded swagger-ui. It will also install a bootstrapper (App_Start/SwaggerConfig.cs) that initiates Swashbuckle on app start-up using [WeActivatorEx](https://github.com/davidebbo/WebActivator). Once installed, you should be able to browse the following raw docs and UI endpoints: 

***\<your-api-endpoint\>/swagger/api-docs***

***\<your-api-endpoint\>/swagger***

### Self-hosted ###

If youre service is self-hosted, you will need to install Swashbuckle.Core directly ...

    Install-Package Swashbuckle.Core

And then manually apply the one-liner to initiate Swashbuckle before starting the server:

    var config = new HttpSelfHostConfiguration("http://localhost:8080");
    Swashbuckle.Bootstrapper.Init(config);
    
### OWIN Self-hosted ###

Similarly, if you're using OWIN to self-host your service, you should install Swashbuckle.Core directly and then manually invoke the Swashbuckle bootstrapper inside the Startup class:

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            Swashbuckle.Bootstrapper.Init(config);

            config.Routes.MapHttpRoute(
                name: "Default",
                routeTemplate: "{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

            app.UseWebApi(config);
        }
    }

### OWIN Hosted in IIS ###

If your using OWIN to host your service via the IIS Integrated pipeline (i.e. Microsoft.Owin.Host.SystemWeb), the steps are the same as above but with some additional workarounds.

1) The following line must be included in your Web.config:

    <configuration>
       <system.webServer>
          <modules runAllManagedModulesForAllRequests=“true” />
       </system.webServer>
    </configuration>
    
This is because IIS has a native module for handling static files and when it sees an extension in the URL, it assumes a static file and tries to handle the request itself skipping remaining parts of the pipeline. However, Swashbuckle serves it's UI routes (which do have extensions) through WebApi which is being invoked through the OWIN module. This setting ensures the OWIN module is run for all requests - extension and extensionless.

2) Add the following stage marker AFTER configuring the WebApi middleware (in namespace Microsoft.Owin.Extensions):

    app.UseStageMarker(PipelineStage.MapHandler);
    
This setting causes the WebApi middleware to execute earlier in the pipeline, allowing it to also handle URL's with extensions. See the following article for more information on stage markers (http://www.asp.net/aspnet/overview/owin-and-katana/owin-middleware-in-the-iis-integrated-pipeline) 

## Troubleshooting ##

Troubleshooting??? I thought this was all supposed to be "seamless"? OK you've called me out! Things shouldn't go wrong, but if they do, take a look at the [troubleshooting steps](#troubleshooting-steps) for inspiration.

## Extensibility ##

Swashbuckle automatically generates a Swagger spec and UI based off the WebApi ApiExplorer. The out-of-the-box generator caters for the majority of WebApi implementations but also includes some extensibility points for application-specific needs ..

### Customize spec generation ###

You can customize the auto-generated spec by applying the following config options at App startup ...

    SwaggerSpecConfig.Customize(c =>
        {
            c.ResolveBasePathUsing((req) => GetBasePathFromAppConfig());
            c.ResolveTargetVersionUsing((req) => "2.0");
            
            c.IgnoreObsoleteActions();
            c.ResolveVersionSupportUsing((apiDesc, version) => GetVersionByAttribute(apiDesc) == version)
            
            c.GroupDeclarationsBy(RootResourceName)

            c.OperationFilter<AddStandardResponseCodes>();
            c.OperationFilter<AddAuthorizationResponseCodes>();
            
            c.MapType<MySerializeableType>(() => new DataType { Type = "string" });

            c.PolymorphicType<Product>(pc => pc
                .DiscriminateBy(p => p.Type)
                .SubType<Book>()
                .SubType<Album>()
                .SubType<Service>(sc => sc
                    .SubType<Shipping>()
                    .SubType<Packaging>()));

            c.IncludeXmlComments(GetXmlCommentsPath());
        });

#### ResolveBasePathUsing ####

Swashbuckle will try to infer your API's base path (authority plus virtual path) from the incoming request (i.e. the request for api-docs). However, there may be situations (e.g. certain load-balanced environments) where this does not resolve correctly. In this case, you can implement your own strategy for resolving your API's base path and wire it up with the **ResolveBasePathUsing** option.

#### ResolveTargetVersionUsing ####

You can use this option to indicate the current version of your API.

It may also be used in conjuction with the **ResolveVersionSupportUsing** to implement a UI that can switch between service versions. For example, something in the Swagger request (maybe a query param) could indicate the target version. Then you can provide a lambda for **ResolveVersionSupportUsing** that takes an ApiDescription and target version and determines if the action should be included in the Swagger spec for that version.

#### IgnoreObsoleteActions ####

Set this option if you'd like to exclude any WebApi actions decorated with the System.ObsoleteAttribute from your swagger spec and UI

#### GroupDeclarationsBy ####

This option accepts a lambda as a strategy for grouping actions into ApiDeclarations. The default implementation groups by controller name. 

#### OperationFilter ####

This provides a way to customize Operation descriptions by applying one or more filters after initial generation. For example:

A filter that enhances the spec with standard response code descriptions ...

    public class AddStandardResponseCodes : IOperationFilter
    {
        public void Apply(Operation operation, DataTypeRegistry dataTypeRegistry, ApiDescription apiDescription)
        {
            operation.ResponseMessages.Add(new ResponseMessage
            {
                Code = (int)HttpStatusCode.OK,
                Message = "It's all good!"
            });

            operation.ResponseMessages.Add(new ResponseMessage
            {
                Code = (int)HttpStatusCode.InternalServerError,
                Message = "Somethings up!"
            });
        }
    }
    
Or, a filter that adds an authorization response description to actions that are decorated with the AuthorizeAttribute ...

    public class AddAuthorizationResponseCodes : IOperationFilter
    {
        public void Apply(Operation operation, DataTypeRegistry dataTypeRegistry, ApiDescription apiDescription)
        {
            if (apiDescription.ActionDescriptor.GetFilters().OfType<AuthorizeAttribute>().Any())
            {
                operation.ResponseMessages.Add(new ResponseMessage
                {
                    Code = (int)HttpStatusCode.Unauthorized,
                    Message = "Authentication required"
                });
            }
        }
    }

The filter interface is relatively simple. In most cases, you just inspect the **apiDescription** and then modify the corresponding **operation** accordingly. If you're customizing DataType descriptions for the operation and need to register new Models for the underlying ApiDeclaration, you can use the provided **dataTypeRegistry**.

#### MapType ####

This allows you to override the default DataType generation for a given Type. It's intended for the use-case when you have a class that is serialized to a primitive JSON type.

#### PolymorphicType ####

The Swagger spec provides a way to describe polymorphic models with **subTypes** and **discriminator** properties. Swashbuckle currently requires these to be explicitly configured. Later versions may include a feature to scan assemblies but in the meantime you can use the fluent "PolymorphicType" method to guide you.  

This will generate an additional complex model for each sub-type and bind them to the base model via the subTypes property:

     "Product": {
       "id": "Product",
       "type": "object",
       "properties": {
         "Id": {
           "type": "integer",
           "format": "int32"
         },
         "Price": {
           "type": "number",
           "format": "double"
         }
         "Type": {
           "type": "string",
         }
       },
       "required": [],
       "subTypes": [ "Book", "Album", "Service" ],
       "discriminator": "Type"
     }

**\*Note:** The [swagger-ui](https://github.com/wordnik/swagger-ui.git) is typically behind the [Swagger Spec](https://github.com/wordnik/swagger-spec/blob/master/versions/1.2.md) and, as of this writing, does not currently have support for displaying polymorphic types.

#### ModelFilter ####

This is similar to the **OperationFilter** option. It provides a way to customize the generated DataType/Model for complex Types in your API. Model filters implement the following interface:

    public interface IModelFilter
    {
        void Apply(DataType model, DataTypeRegistry dataTypeRegistry, Type type);
    }

#### IncludeXmlComments ####

If you annonate Controllers and API Types with [Xml Comments](http://msdn.microsoft.com/en-us/library/b2s063f7(v=vs.110).aspx), you can use this option to incorporate those comments into the generated spec and UI. The Xml tags are mapped to Swagger properties as follows:

* **Action summary** -> Operation.Summary
* **Action remarks** -> Operation.Notes
* **Parameter summary** -> Operation.Parameters[name].Description
* **Type summary** -> DataType.Descripton
* **Property summary** -> DataType.Properties[name].Description

Although it's not one of the official XML comment tags, Swashbuckle also supports the use of one or more **response** tags on an action. These can be used to describe the error codes for a given operation. For example,

    /// <response code="200">It's all good!</response>
    /// <response code="500">Somethings up!</response>
    public int Create(Product product)
    
These values will get mapped to the Operation.ResponseMessages.

### Customize the swagger-ui ###

The Swagger UI supports a number of options to customize it's appearance and behavior. See the [documentation](https://github.com/wordnik/swagger-ui) for a detailed description.

All of these options are exposed through Swashbuckle configuration ...

    SwaggerUiConfig.Customize(c =>
        {
            c.SupportHeaderParams = true;
            c.DocExpansion = DocExpansion.List;
            c.SupportedSubmitMethods = new[] {HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Head};
            c.InjectJavaScript(typeof (SwaggerConfig).Assembly, "Swashbuckle.TestApp.SwaggerExtensions.onComplete.js");
            c.InjectStylesheet(typeof(SwaggerConfig).Assembly, "Swashbuckle.TestApp.SwaggerExtensions.customStyles.css");
            
            c.CustomRoute("index.html", resourceAssembly, "Swashbuckle.TestApp.SwaggerExtensions.myIndex.html");
        });

The **InjectJavaScript** and **InjectStylesheet** options allow custom JavaScript or CSS to be injected into the UI once it's loaded.

To do this, the file(s) **MUST** be added to your project as an "Embedded Resource". After that, you can inject them by specifying the containing Assembly and resource name as shown above.

**What's the resource name?**: When you add an embedded resource (Right-click in Solution Explorer -> Properties -> Build Action), it is assigned the following resource name by default:

\<Project Default Namespace>.\<Escaped Folder Path>.\<File Name>

So, if your app's default namespace is "Swashbuckle.TestApp", and you have a custom script - "SwaggerExtensions/onComplete.js", then it will be assigned the following resource name at build time:

"Swashbuckle.TestApp.SwaggerExtensions.onComplete.js"

#### Custom ui routes and embedded resources ####

You can use the **CustomRoute** option to map a swagger ui path (<your-api-endpoint\>/swagger/ui/{*uiPath}) to 
a custom embedded resource. For example, the code sample above overrides index.html from the embedded swagger-ui with a custom version. You could use this approach to build your own tailored version of the swagger-ui and then serve it up instead of the default embedded version.

## Transitioning to Swashbuckle 4.0 ##

If you're upgrading from a version prior to 4.0, the following information will help make the upgrade as seamless as possible

### Changes to Bootstrapping ###

Because Swashbuckle 4.0 has no dependency on ASP.Net MVC, the approach to bootstrapping is a little different. The upgrade should install a bootstrapper (App_Start/SwaggerConfig.cs) that is invoked on app start-up using [WeActivatorEx](https://github.com/davidebbo/WebActivator). If you're unable to access the Swagger content after upgrading, check out the [bootstrap troubleshooting steps](#missing-bootststrap-one-liner-only-applicable-to-40-and-above).

### Changes to the Config API ###

* SwaggerSpecConfig and SwaggerUiConfig classes have moved namespace from Swashbuckle.Models to Swashbuckle.Application.
* ApiVersion has been replaced with a method - ResolveTargetVersionUsing that accepts a lambda.
* IgnoreObsoleteActions has been converted to a method.
* IOperationFilter interface has moved namespace from Swashbuckle.Models to Swashbuckle.Swagger
* It's signature has also been simplified - see the [Extensibility](#operationfilter) section for more details
* Some of the Swagger types have been renamed to better reflect terminology used in the [Swagger Spec](https://github.com/wordnik/swagger-spec/blob/master/versions/1.2.md)
    * the "Spec" suffix has been removed from several of the class names, e.g. OperationSpec is now called Operation
    * The ModelSpec class has been renamed to DataType as it describes both primitive and complex data types (Models).

### Leveraging Xml Comments ###

In previous versions this was left up to the app - typically with a custom implementation of the WebApi's IDocumentationProvider and an IOperationFilter that parses the XML into the relevant Swagger properties
. Version 4.0 supports this out-of-the-box and so both of these artifacts can be removed from your app. See the [Extensibilty](#extensibility) section for details on including Xml Comments.

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
