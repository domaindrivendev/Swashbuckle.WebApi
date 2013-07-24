using System.Collections.Generic;

namespace Swashbuckle.Models
{
    public interface ISwaggerSpec
    {
        ResourceListing GetResourceListing();

        ApiDeclaration GetApiDeclaration(string resourcePath);
    }

    public class ResourceListing
    {
        public string apiVersion { get; set; }
        public string swaggerVersion { get; set; }
        public string basePath { get; set; }
        public ICollection<ApiDeclarationLink> apis { get; set; }
    }

    public class ApiDeclarationLink
    {
        public string path { get; set; }
    }

    public class ApiDeclaration
    {
        public string apiVersion { get; set; }
        public string swaggerVersion { get; set; }
        public string basePath { get; set; }
        public string resourcePath { get; set; }
        public ICollection<ApiSpec> apis { get; set; }
        public Dictionary<string, ModelSpec> models { get; set; }
    }

    public class ApiSpec
    {
        public string path { get; set; }
        public string description { get; set; }
        public ICollection<ApiOperationSpec> operations { get; set; }
    }

    public class ApiOperationSpec
    {
        public string httpMethod { get; set; }
        public string nickname { get; set; }
        public string summary { get; set; }
        public string responseClass { get; set; }
        public ICollection<ApiParameterSpec> parameters { get; set; }
        public ICollection<ApiErrorResponseSpec> errorResponses { get; set; }
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