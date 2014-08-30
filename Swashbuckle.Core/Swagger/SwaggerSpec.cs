using System.Collections.Generic;
using Newtonsoft.Json;

namespace Swashbuckle.Swagger
{
    public class ResourceListing
    {
        [JsonProperty("swaggerVersion")]
        public string SwaggerVersion { get; set; }

        [JsonProperty("apis")]
        public IList<Resource> Apis { get; set; }

        [JsonProperty("apiVersion")]
        public string ApiVersion { get; set; }

        [JsonProperty("info")]
        public Info Info { get; set; }

        [JsonProperty("authorizations")]
        public IDictionary<string, Authorization> Authorizations { get; set; }
    }

    public class Resource
    {
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class Info
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("termsOfServiceUrl")]
        public string TermsOfServiceUrl { get; set; }

        [JsonProperty("contact")]
        public string Contact { get; set; }

        [JsonProperty("license")]
        public string License { get; set; }

        [JsonProperty("licenseUrl")]
        public string LicenseUrl { get; set; }
    }

    public class Authorization
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("passAs")]
        public string PassAs { get; set; }

        [JsonProperty("keyname")]
        public string KeyName { get; set; }

        [JsonProperty("scopes")]
        public IList<Scope> Scopes { get; set; }

        [JsonProperty("grantTypes")]
        public GrantTypes GrantTypes { get; set; }
    }

    public class Scope
    {
        [JsonProperty("scope")]
        public string ScopeId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class GrantTypes
    {
        [JsonProperty("implicit")]
        public ImplicitGrant ImplicitGrant { get; set; }

        [JsonProperty("authorization_code")]
        public AuthorizationCodeGrant AuthorizationCode { get; set; }
    }

    public class ImplicitGrant
    {
        [JsonProperty("loginEndpoint")]
        public LoginEndpoint LoginEndpoint { get; set; }

        [JsonProperty("tokenName")]
        public string TokenName { get; set; }
    }

    public class LoginEndpoint
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class AuthorizationCodeGrant
    {
        [JsonProperty("tokenRequestEndpoint")]
        public TokenRequestEndpoint TokenRequestEndpoint { get; set; }

        [JsonProperty("tokenEndpoint")]
        public TokenEndpoint TokenEndpoint { get; set; }
    }

    public class TokenRequestEndpoint
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("clientIdName")]
        public string ClientIdName { get; set; }

        [JsonProperty("clientSecretName")]
        public string ClientSecretName { get; set; }
    }

    public class TokenEndpoint
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("tokenName")]
        public string TokenName { get; set; }
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
        public IDictionary<string, Model> Models { get; set; }

        [JsonProperty("produces")]
        public IList<string> Produces { get; set; }

        [JsonProperty("consumes")]
        public IList<string> Consumes { get; set; }

        [JsonProperty("authorizations")]
        public Dictionary<string, IList<Scope>> Authorizations { get; set; }
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

    public class Operation : DataType
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("authorizations")]
        public Dictionary<string, IList<Scope>> Authorizations { get; set; }

        [JsonProperty("parameters")]
        public IList<Parameter> Parameters { get; set; }

        [JsonProperty("responseMessages")]
        public IList<ResponseMessage> ResponseMessages { get; set; }

        [JsonProperty("produces")]
        public IList<string> Produces { get; set; }

        [JsonProperty("consumes")]
        public IList<string> Consumes { get; set; }
    }

    public class Parameter : DataType
    {
        [JsonProperty("paramType")]
        public string ParamType { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("required")]
        public bool Required { get; set; }

        [JsonProperty("allowMultiple")]
        public bool? AllowMultiple { get; set; }
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
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("$ref")]
        public string Ref { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("defaultValue")]
        public string DefaultValue { get; set; }

        [JsonProperty("enum")]
        public IList<string> Enum { get; set; }

        [JsonProperty("minimum")]
        public string Minimum { get; set; }

        [JsonProperty("maximum")]
        public string Maximum { get; set; }

        [JsonProperty("items")]
        public DataType Items { get; set; }

        [JsonProperty("uniqueItems")]
        public bool? UniqueItems { get; set; }
    }

    public class Model
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("required")]
        public IList<string> Required { get; set; }

        [JsonProperty("properties")]
        public IDictionary<string, Property> Properties { get; set; }

        [JsonProperty("subTypes")]
        public IList<string> SubTypes { get; set; }

        [JsonProperty("discriminator")]
        public string Discriminator { get; set; }
    }

    public class Property : DataType
    {
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}