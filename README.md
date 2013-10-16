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

For example you can customize the auto-generated spec by applying the following config options at App startup ...

    SwaggerSpecConfig.Customize(c =>
        {
            c.GroupDeclarationsBy(GetRootResource);
            c.PostFilter<AddStandardResponseMessages>();
            c.PostFilter<AddAuthorizationResponseMessages>();
            c.MapType<MySerializableType>(new ModelSpec { Type = "string" });
        });
        
#### GroupDeclarationsBy ####

This option accepts a lambda as a strategy for grouping api's into ApiDeclarations. The default implementation groups by controller name. 

#### PostFilter ####

You can use this option to apply any number of filters that modify each "OperationSpec" after initial generation. For example ...

A filter that enhances the spec with standard response code descriptions:

    public class AddStandardResponseMessages : IOperationSpecFilter
    {
        public void Apply(ApiDescription apiDescription, OperationSpec operationSpec, ModelSpecMap modelSpecMap)
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
        public void Apply(ApiDescription apiDescription, OperationSpec operationSpec, ModelSpecMap modelSpecMap)
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

#### MapType ####

This option accepts a Type and a ModelSpec. It allows you to customize the ModelSpec for a given Type. 

### Customize the swagger-ui ###

The Swagger UI supports a number of options to customize it's appearance and behavior. See the [documentation](https://github.com/wordnik/swagger-ui) for a detailed description.

All of these options are exposed through Swashbuckle configuration ...

    SwaggerUiConfig.Customize(c =>
        {
            c.SupportHeaderParams = true;
            c.DocExpansion = DocExpansion.List;
            c.SupportedSubmitMethods = new[] {HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Head};
            c.AddOnCompleteScript(typeof (SwaggerConfig).Assembly, "Swashbuckle.TestApp.SwaggerExtensions.onComplete.js");
            c.AddStylesheet(typeof(SwaggerConfig).Assembly, "Swashbuckle.TestApp.SwaggerExtensions.customStyles.css");
        });

The __AddOnCompleteScript__ and __AddStylesheet__ options allow custom JavaScript or CSS to be injected into the UI once it's loaded.

To do this, the file(s) must be added to your project as an "Embedded Resource". Then, you can inject them by specifying the containing Assembly and resource name as shown above.

__TIP__: When you add an embedded resource (Right-click in Solution Explorer -> Properties -> Build Action), it is assigned the following name by default:

\<Project Default Namespace>.\<Escaped Folder Path>.\<File Name>

For example, if your app's default namespace is "Swashbuckle.TestApp", and you want to include a script at the following path within your project - "SwaggerExtensions/onComplete.js", then it will be assigned the following Logical Name at build time:

"Swashbuckle.TestApp.SwaggerExtensions.onComplete.js"

### Extract documentation from XML Comments ###

WebApi ships with an [ApiExplorer](http://msdn.microsoft.com/en-us/library/system.web.http.description.apiexplorer.aspx) component that provides a service description based off routes, controllers and actions. Swashbuckle then uses this to generate a corresponding Swagger spec.

So, to have Swashbuckle pull descriptive fields from XML Comments in code, you'll need to start by wiring up this functionailty in ApiExplorer. This is done by implementing a custom IDocumentationProvider. Step 3) in this [blog](http://blogs.msdn.com/b/yaohuang1/archive/2012/05/21/asp-net-web-api-generating-a-web-api-help-page-using-apiexplorer.aspx) provides more details. The implementation described here extracts the "summary" node from XML Comments and ApiExplorer in turn set's this value on the ApiDescription.Documentation field. Finally, Swashbuckle maps this value to the "summary" field of the corresponding OperationSpec.

However, Swagger also provides a second "notes" field for providing additional information on an operation. If you'd like to source both fields from the XML Comments, you can modify the implementation of XmlCommentsDocumentationProvider to return the whole XML block instead of just the summary node. Then, you can implement an operation spec filter to parse the XML and assign both the "summary" and "notes" fields accordingly:

    public class ExtractXmlComments : IOperationSpecFilter
    {
        public void Apply(ApiDescription apiDescription, OperationSpec operationSpec, ModelSpecMap modelSpecMap)
        {
            var descriptionXml = XElement.Parse(apiDescription.Documentation);

            var summary = descriptionXml.Element("summary");
            operationSpec.Summary = summary != null ? summary.Value : descriptionXml.Value;

            var notes = descriptionXml.Element("remarks");
            if (notes != null)
                operationSpec.Notes = notes.Value;
        }
    }
