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
        public IDictionary<string, ModelSpec> models { get; set; }
    }

    public class ApiSpec
    {
        public string path { get; set; }
        public string description { get; set; }
        public ICollection<OperationSpec> operations { get; set; }
    }

    public class OperationSpec
    {
        public string method { get; set; }
        public string nickname { get; set; }
        public string summary { get; set; }
        public string notes { get; set; }
        public string type { get; set; }
        public IEnumerable<string> @enum { get; set; }
        public IDictionary<string, string> items { get; set; }
        public ICollection<ParameterSpec> parameters { get; set; }
        public ICollection<ResponseMessageSpec> responseMessages { get; set; }
    }

    public class ParameterSpec
    {
        public string paramType { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public bool required { get; set; }
        public string type { get; set; }
        public IEnumerable<string> @enum { get; set; }
        public IDictionary<string, string> items { get; set; }
    }

    public class ResponseMessageSpec
    {
        public int code { get; set; }
        public string message { get; set; }
    }

    public class ModelSpec
    {
        public string id { get; set; }
        public string type { get; set; }
        public string format { get; set; }
        public IEnumerable<string> @enum { get; set; }
        public IDictionary<string, string> items { get; set; }
        public IDictionary<string, ModelSpec> properties { get; set; }
    }
}