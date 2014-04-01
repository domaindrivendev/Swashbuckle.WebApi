using System.Collections.Generic;
using Newtonsoft.Json;

namespace Swashbuckle.Core.Swagger
{
    public class SwaggerSpec
    {
        public ResourceListing Listing { get; set; }

        public Dictionary<string, ApiDeclaration> Declarations { get; set; }
    }

    public class ResourceListing
    {
        [JsonProperty("swaggerVersion")]
        public string SwaggerVersion { get; set; }

        [JsonProperty("apiVersion")]
        public string ApiVersion { get; set; }

        [JsonProperty("apis")]
        public IList<ApiDeclarationLink> Apis { get; set; }
    }

    public class ApiDeclarationLink
    {
        [JsonProperty("path")]
        public string Path { get; set; }
    }

    public class ApiDeclaration
    {
        [JsonProperty("swaggerVersion")]
        public string SwaggerVersion { get; set; }

        [JsonProperty("apiVersion")]
        public string ApiVersion { get; set; }

        [JsonProperty("basePath")]
        public string BasePath { get; set; }

        [JsonProperty("resourcePath")]
        public string ResourcePath { get; set; }

        [JsonProperty("apis")]
        public IList<ApiSpec> Apis { get; set; }

        [JsonProperty("models")]
        public IDictionary<string, ModelSpec> Models { get; set; }
    }

    public class ApiSpec
    {
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("operations")]
        public IList<OperationSpec> Operations { get; set; }
    }

    public class OperationSpec
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("items")]
        public ModelSpec Items { get; set; }

        [JsonProperty("enum")]
        public IList<string> Enum { get; set; }

        [JsonProperty("parameters")]
        public IList<ParameterSpec> Parameters { get; set; }

        [JsonProperty("responseMessages")]
        public IList<ResponseMessageSpec> ResponseMessages { get; set; }
    }

    public class ParameterSpec
    {
        [JsonProperty("paramType")]
        public string ParamType { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("required")]
        public bool Required { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("items")]
        public ModelSpec Items { get; set; }

        [JsonProperty("enum")]
        public IList<string> Enum { get; set; }
    }

    public class ResponseMessageSpec
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class ModelSpec
    {
        [JsonProperty("$ref")]
        public string Ref { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("items")]
        public ModelSpec Items { get; set; }

        [JsonProperty("enum")]
        public IList<string> Enum { get; set; }

        [JsonProperty("properties")]
        public IDictionary<string, ModelSpec> Properties { get; set; }

        [JsonProperty("required")]
        public IList<string> Required { get; set; }

        [JsonProperty("subTypes")]
        public IList<string> SubTypes { get; set; }

        [JsonProperty("discriminator")]
        public string Discriminator { get; set; }
    }
}
