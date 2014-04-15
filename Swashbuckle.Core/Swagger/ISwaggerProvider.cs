using System.Collections.Generic;
using Newtonsoft.Json;

namespace Swashbuckle.Core.Swagger
{
    public interface ISwaggerProvider
    {
        ResourceListing GetListing(string basePath, string version);

        ApiDeclaration GetDeclaration(string basePath, string version, string resourceName);
    }

    public class ResourceListing
    {
        [JsonProperty("swaggerVersion")]
        public string SwaggerVersion { get; set; }

        [JsonProperty("apiVersion")]
        public string ApiVersion { get; set; }

        [JsonProperty("apis")]
        public IList<Resource> Apis { get; set; }
    }

    public class Resource
    {
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
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
        public IList<Api> Apis { get; set; }

        [JsonProperty("models")]
        public IDictionary<string, DataType> Models { get; set; }
    }

    public class Api
    {
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("operations")]
        public IList<Operation> Operations { get; set; }
    }

    public class Operation
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
        public DataType Items { get; set; }

        [JsonProperty("enum")]
        public IList<string> Enum { get; set; }

        [JsonProperty("parameters")]
        public IList<Parameter> Parameters { get; set; }

        [JsonProperty("responseMessages")]
        public IList<ResponseMessage> ResponseMessages { get; set; }
    }

    public class Parameter
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
        public DataType Items { get; set; }

        [JsonProperty("enum")]
        public IList<string> Enum { get; set; }
    }

    public class ResponseMessage
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class DataType
    {
        [JsonProperty("$ref")]
        public string Ref { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("items")]
        public DataType Items { get; set; }

        [JsonProperty("enum")]
        public IList<string> Enum { get; set; }

        [JsonProperty("properties")]
        public IDictionary<string, DataType> Properties { get; set; }

        [JsonProperty("required")]
        public IList<string> Required { get; set; }

        [JsonProperty("subTypes")]
        public IList<string> SubTypes { get; set; }

        [JsonProperty("discriminator")]
        public string Discriminator { get; set; }
    }
}