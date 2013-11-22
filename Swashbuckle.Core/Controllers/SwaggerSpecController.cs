using System;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using Newtonsoft.Json;
using Swashbuckle.Core.Models;

namespace Swashbuckle.Core.Controllers
{
    [ApiDocsConfig]
    public class SwaggerSpecController : ApiController
    {
        public ResourceListing GetListing()
        {
            var swaggerSpec = SwaggerSpec.GetInstance(Configuration.Services.GetApiExplorer(), Request);
            return swaggerSpec.Listing;
        }

        public ApiDeclaration GetDeclaration(string name)
        {
            var swaggerSpec = SwaggerSpec.GetInstance(Configuration.Services.GetApiExplorer(), Request);
            return swaggerSpec.Declarations["/" + name];
        }
    }

    public class ApiDocsConfigAttribute : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            controllerSettings.Formatters.Clear();

            var jsonFormatter = new JsonMediaTypeFormatter
                {
                    SerializerSettings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore}
                };
            controllerSettings.Formatters.Add(jsonFormatter);
        }
    }
}
