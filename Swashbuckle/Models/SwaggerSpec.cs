using System.Collections.Generic;

namespace Swashbuckle.Models
{
    public class SwaggerSpec
    {
        public ResourceListing Listing { get; set; }

        public Dictionary<string, ApiDeclaration> Declarations { get; set; } 
    }

    public class ResourceListing
    {
        public string ApiVersion { get; set; }
        public string SwaggerVersion { get; set; }
        public ICollection<ApiDeclarationLink> Apis { get; set; }
    }

    public class ApiDeclarationLink
    {
        public string Path { get; set; }
    }

    public class ApiDeclaration
    {
        public string ApiVersion { get; set; }
        public string SwaggerVersion { get; set; }
        public string BasePath { get; set; }
        public string ResourcePath { get; set; }
        public ICollection<ApiSpec> Apis { get; set; }
        public Dictionary<string, ModelSpec> Models { get; set; }
    }

    public class ApiSpec
    {
        public string Path { get; set; }
        public string Description { get; set; }
        public ICollection<OperationSpec> Operations { get; set; }
    }

    public class OperationSpec
    {
        public string Method { get; set; }
        public string Nickname { get; set; }
        public string Summary { get; set; }
        public string Notes { get; set; }
        public string Type { get; set; }
        public string Format { get; set; }
        public ModelSpec Items { get; set; }
        public ICollection<string> Enum { get; set; } 
        public ICollection<ParameterSpec> Parameters { get; set; }
        public ICollection<ResponseMessageSpec> ResponseMessages { get; set; }
    }

    public class ParameterSpec
    {
        public string ParamType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public string Type { get; set; }
        public string Format { get; set; }
        public ModelSpec Items { get; set; }
        public ICollection<string> Enum { get; set; } 
    }

    public class ResponseMessageSpec
    {
        public int Code { get; set; }
        public string Message { get; set; }
    }

    public class ModelSpec
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Format { get; set; }
        public ModelSpec Items { get; set; }
        public ICollection<string> Enum { get; set; } 
        public Dictionary<string, ModelSpec> Properties { get; set; }
    }
}
