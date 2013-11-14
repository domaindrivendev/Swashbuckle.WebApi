using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;
using Swashbuckle.Core.Models;

namespace SwashBuckle.WebApi.Controller
{
    [JsonResponseConfigAttribute]
    public class ApiDocsController : ApiController
    {
        private readonly SwaggerSpec _swaggerSpec;
        

        public ApiDocsController()
        {
            var apiExplorer = GlobalConfiguration.Configuration.Services.GetApiExplorer();
            System.Diagnostics.Debug.WriteLine(apiExplorer.ApiDescriptions.Count);
            _swaggerSpec = SwaggerGenerator.Instance.Generate(apiExplorer);
        }

        [HttpGet]
        public ResourceListing Index()
        {
            return _swaggerSpec.Listing;
        }

        [HttpGet]
        public ApiDeclaration Show(string resourceName)
        {
            var resourcePath = "/" + resourceName;
            return _swaggerSpec.Declarations[resourcePath];
        }
    }
}
