Swashbuckle
=========
Seamlessly adds a [Swagger](https://developers.helloreverb.com/swagger) to WebApi projects! Uses a combination of ApiExplorer and Swagger/Swagger-UI to provide a rich discovery and documentation experience for consumers.

The library comes packaged with the neccessary UI components including HTML files, JavaScript and CSS. This reduces unnecessary noise and maintenance in your WebApi project, leaving you free to focus on building an awesome API!   

And that's not all ...

Once you have a Web API that can describe itself in Swagger, you've opened the treasure chest of Swagger-based tools including a client generator that can be targetted to a wide range of popular platforms. See [Swagger-Codegen](https://github.com/wordnik/swagger-codegen) for more details.

**UPDATE:** As of version 3.0 (Nuget), Swashbuckle emits version 1.2 of the Swagger Spec. See [Swagger 1.2](https://github.com/wordnik/swagger-core/wiki/1.2-transition) for a list of changes.

Getting Started
--------------------

To start exposing auto-generated Swagger docs and a Swagger UI, simply install the Nuget package from your WebApi project:

    Install-Package Swashbuckle

This will add a reference to Swashbuckle.dll which contains an embedded "Area" for Swagger. The Area includes the following enpoints for the raw swagger docs and ui respectively:

*swagger/api-docs*

*swagger*

Extensibility
--------------------

Swashbuckle automtically generates a Swagger spec and UI based off the WebApi ApiExplorer. The out-of-the-box generator caters for the majority of WebApi implementations but also includes some extensibility points for application-specific needs ..

### Customize spec generation ###

For example you can customize the auto-generated spec by applying several (optional) config options at App startup ...

    SwaggerSpecConfig.Customize(c =>
        {
            c.GroupDeclarationsBy(GetRootResource);
            c.PostFilter<AddStandardResponseMessages>();
            c.PostFilter<AddAuthorizationResponseMessages>();
        });
        
#### GroupDeclarationsBy ####

This option accepts a lambda as a strategy for grouping api's into ApiDeclarations. The default uses controller name as the grouping key. 

#### PostFilter ####

You can use this option to apply any number of filters that modify "OperationSpec's" after initial generation. For example ...

A filter that enhances the spec with standard response code descriptions:

    public class AddStandardResponseMessages : IOperationSpecFilter
    {
        public void Apply(ApiDescription apiDescription, OperationSpec operationSpec)
        {
            operationSpec.ResponseMessages.Add(new ResponseMessageSpec
                {
                    Code = (int) HttpStatusCode.OK,
                    Message = "It's all good!"
                });

            operationSpec.ResponseMessages.Add(new ResponseMessageSpec
            {
                Code = (int)HttpStatusCode.InternalServerError,
                Message = "Somethings up!"
            });
        }
    }
    
Or, a filter that adds an authorization response code description to actions that are decorated with the AuthorizeAttribute:

    public class AddAuthorizationResponseMessages : IOperationSpecFilter
    {
        public void Apply(ApiDescription apiDescription, OperationSpec operationSpec)
        {
            if (apiDescription.ActionDescriptor.GetFilters().OfType<AuthorizeAttribute>().Any())
            {
                operationSpec.ResponseMessages.Add(new ResponseMessageSpec
                    {
                        Code = (int) HttpStatusCode.Unauthorized,
                        Message = "Authentication required"
                    });
            }
        }
    }

__NOTE:__ See Appendix A for another example that uses an IOperationSpecFilter as part of a solution for providing richer documentation with XML Comments.

##### 1) Describe additional parameters and/or response messages #####

Consider an API that controls access to certain actions by requiring it's callers to authenticate themeselves. WebApi provides a number of 



The client must send additional data to authenticate themeselves (often in the HTTP Authorization header). So, to describe the API accurately


To account for this, the OperationSpec for any restricted action should be updated accordingly. This means indicating the additional parameter (i.e. the Authorization header), and describing the potential error codes. This can be done with OperationSpecFilter's as follows:

    SwaggerSpecConfig.Customize(c =>
    {
        c.PostFilter<AddAuthorizationHeaderParameter>();
        c.PostFilter<AddAuthorizationErrorCodes>();
    });

The filters can inspect the action for the presence of the AuthorizeAttribute and update the OperationSpec accordingly.

    public class AddAuthorizationErrorCodes : IOperationSpecFilter
    {
        public void Apply(ApiDescription apiDescription, OperationSpec operationSpec)
        {
            if (apiDescription.ActionDescriptor.GetFilters().OfType<AuthorizeAttribute>().Any())
            {
                operationSpec.ResponseMessages.Add(new ResponseMessageSpec
                {
                    Code = (int)HttpStatusCode.Unauthorized,
                    Message = "Basic Auth required"
                });
            }
        }
    }

There may be cases when certain behavior

    SwaggerSpecConfig.Customize(c =>
        {
            c.GroupDeclarationsBy(GetRootResource);
            c.PostFilter(new AddErrorCodeFilter(200, "It's all good!"));
            c.PostFilter(new AddErrorCodeFilter(400, "Something's up!"));
        });

In addition, the [Swagger-UI](https://github.com/wordnik/swagger-ui) supports a number of options to customize it's appearance and behavior. All of these config options are exposed through Swashbuckle.

    SwaggerUiConfig.Customize(c =>
        {
            c.SupportHeaderParams = true;
            c.DocExpansion = DocExpansion.List;
            c.SupportedSubmitMethods = new[] {HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Head};
            c.AddOnCompleteScript(typeof (SwaggerConfig).Assembly, "Swashbuckle.TestApp.SwaggerExtensions.onComplete.js");
            c.AddStylesheet(typeof(SwaggerConfig).Assembly, "Swashbuckle.TestApp.SwaggerExtensions.customStyles.css");
        });

See below for some common examples ...

### 1. Group api declarations by root resource ###

By default, Swashbuckle will group api's into ApiDeclaration's by controller name. If the controller-per-resource convention is adhered to, this will amount to an ApiDeclaration per resource as suggested by the Swagger documentation. However, this convention doesn't always make sense, particularly when default WebApi routing isn't being used. To get around this, you can provide a custom stratgey for assigning api's to ApiDeclaration's:

    public static void Customize()
    {
        SwaggerSpecConfig.Customize(c =>
            {
                c.GroupDeclarationsBy(GetRootResource);
            });
    }

    private static string GetRootResource(ApiDescription apiDescription)
    {
        var path = apiDescription.RelativePath.Replace("api/", "");
        var cutoffIndex = path.Contains("/") ? path.IndexOf('/') : path.IndexOf('?');
        return (cutoffIndex < 0) ? path : path.Substring(0, cutoffIndex);
    }
    
### 2. Ammend the generated spec with error codes ###

To ammend OperationSpec's after initial generation, you can implement and apply one or more "OperationSpecFilter's".

For example, consider an API that requires Basic Authorization. In WebApi, this can be implemented in a number of ways, but typically requires that the client sends credentials via the HTTP Authorization header. Then restricted actions are decorated with the AuthorizeAttribute.

To account for this, the OperationSpec for any restricted action should be updated accordingly. This means indicating the additional parameter (i.e. the Authorization header), and describing the potential error codes. This can be done with OperationSpecFilter's as follows:

    SwaggerSpecConfig.Customize(c =>
    {
        c.PostFilter<AddAuthorizationHeaderParameter>();
        c.PostFilter<AddAuthorizationErrorCodes>();
    });

The filters can inspect the action for the presence of the AuthorizeAttribute and update the OperationSpec accordingly.

    public class AddAuthorizationErrorCodes : IOperationSpecFilter
    {
        public void Apply(ApiDescription apiDescription, OperationSpec operationSpec)
        {
            if (apiDescription.ActionDescriptor.GetFilters().OfType<AuthorizeAttribute>().Any())
            {
                operationSpec.ResponseMessages.Add(new ResponseMessageSpec
                {
                    Code = (int)HttpStatusCode.Unauthorized,
                    Message = "Basic Auth required"
                });
            }
        }
    }

### 2. Provide summary and notes using XML comments ###

TODO:

# swagger-ui customizations

The Swagger UI supports a number of customizations that can be applied by modifying the JavaScript in index.html. Each setting is explained in the [documentation](https://github.com/wordnik/swagger-ui).

In Swashbuckle however, this file is not accessible as it's embedded in the library rather than copied to your application. So, Swashbuckle includes a configuration API that allows you to customize each of these settings on application start up.

    SwaggerUiConfig.Customize(c =>
    {
        c.SupportHeaderParams = true;
        c.DocExpansion = DocExpansion.List;
        c.SupportedSubmitMethods = new[] {HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Head, HttpMethod.Delete};
        c.AddOnCompleteScript(typeof(Bootstrapper).Assembly, "YourApp.swagger_ui.ext.after-load.js");
        c.AddStylesheet(typeof(Bootstrapper).Assembly, "YourApp.swagger_ui.ext.stylesheet.css");
    });
       
It's possible to include one or more custom JavaScript files to be executed as soon as the UI is rendered. You can use this to add additional UI components through the DOM. The UI itself depends on jquery and so it can be used in your custom script.

To add a custom script, you need to include it in your project as an Embedded Resource. You can then inject it by calling AddOnCopmpleteScript as shown above. The first parameter is the assembly which contains the Embedded Resource and the second is the Logical Name of that resource. When you add a file through Visual Studio and change it's Build Action property to "Embedded Resource", it will have the following Logical Name:

\<Project Default Namespace>.\<Escaped Folder Path>.\<File Name>

For example, if your app's default namespace is "YourApp", and you want to include a script at the following path within your project - "swagger-ui/ext/after-load.js", then it will, by default, be assigned the following Logical Name at build time:

"YourApp.swagger_ui.ext.after-load.js"

Same approach applies to custom stylesheets. Unlike the injected scripts, which are executed only after the UI is rendered, injected stylesheets appear as <link rel='stylesheet' ... /> elements in the <head> tag, so they are loaded prior to UI rendering. This extension point may be used for altering the appearance of the UI itself in a simpler way.

To add a custom stylesheet, you need to include it as an Embedded Resource, and treat it as described above for JavaScript includes.
        
## Ammend generated operation specs 

There may also be cases where you'd like to make ammendments to the spec after it's been generated. One example would be adding some app specific error codes for each individual operation.

To do this, Swashbuckle provides a separate configuration where you can specify one or more filters to be applied to the generated operation specs:

    SwaggerSpecConfig.Customize(c =>
    {
        c.PostFilter<ApplyHeaderParamsFilter>();
        c.PostFilter<ApplyErrorCodesFilter>();
    });
    
By implementing the IOperationSpecFilter interface, you can write filters that hook into the spec generation process. The code below shows an example that adds error codes based on the AuthorizeAttribute:

    public class ApplyErrorCodesFilter : IOperationSpecFilter
    {
        public void Apply(ApiDescription apiDescription, ApiOperationSpec operationSpec)
        {
            if (apiDescription.ActionDescriptor.GetFilters().OfType<AuthorizeAttribute>().Any())
            {
                operationSpec.errorResponses.Add(new ApiErrorResponseSpec {code = (int) HttpStatusCode.Unauthorized, reason = "Basic Auth required"});
            }
        }
    }

Another example would be using  XML comments to document API calls. Assume we've got a method commented like this:

    /// <summary> Get all foo's for particular bar </summary>
    /// <param name="barId"> bar identifier </param>
    /// <remarks>Returns all three order items we've got here</remarks>
    /// <response code="200">OK</response>
    public List<Foo> GetAll(int barId) {...}
    
One can implement IDocumentationProvider to read info from these comments, and substite the default one for ApiExplorer. While ApiDescription only has one string field available to put all info, one can serialize desirable information to xml once again (or csv, or json), and then implement IOperationSpecFilter to deserialize it and populate the fields needed:

    var descriptionXml = XElement.Parse(apiDescription.Documentation);

    var notes = descriptionXml.Element("remarks");
    if (notes != null)
        operationSpec.notes = notes.Value;

And so on. The above examles are included in a Swashbuckle.TestApp project.
