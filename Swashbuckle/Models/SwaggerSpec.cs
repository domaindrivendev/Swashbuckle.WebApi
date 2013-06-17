using System.Collections.Generic;

namespace Swashbuckle.Models
{
    public class SwaggerSpec
    {
        public ResourceListing ResourceListing { get; set; }

        public Dictionary<string, ApiDeclaration> ApiDeclarations { get; set; } 
    }

    public class ResourceListing
    {
        public string apiVersion { get; set; }
        public string swaggerVersion { get; set; }
        public string basePath { get; set; }
        public IEnumerable<ResourceLink> apis { get; set; }
    }

    public class ResourceLink
    {
        public string path { get; set; }
    }

    public class ApiDeclaration
    {
        public string apiVersion { get; set; }
        public string swaggerVersion { get; set; }
        public string basePath { get; set; }
        public string resourcePath { get; set; }
        public IEnumerable<ApiSpec> apis { get; set; }
        public Dictionary<string, ModelSpec> models { get; set; }
    }

    public class ApiSpec
    {
        public string path { get; set; }
        public string description { get; set; }
        public IEnumerable<ApiOperationSpec> operations { get; set; }
    }

    public class ApiOperationSpec
    {
        public string httpMethod { get; set; }
        public string summary { get; set; }
        public string responseClass { get; set; }
        public string nickname { get; set; }
        public IEnumerable<ApiParameterSpec> parameters { get; set; }
        public IEnumerable<ApiErrorResponseSpec> errorResponses { get; set; }
    }

    public class ApiParameterSpec
    {
        public string paramType { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string dataType { get; set; }
        public bool required { get; set; }
        public bool allowMultiple { get; set; }
    }

    public class ApiErrorResponseSpec
    {
        public int code { get; set; }
        public string reason { get; set; }
    }

    public class ModelSpec
    {
        public string id { get; set; }
        public Dictionary<string, ModelPropertySpec> properties { get; set; }
    }

    public class ModelPropertySpec
    {
        public string type { get; set; }
        public bool required { get; set; }
    }
}