using System.Collections.Generic;
using Newtonsoft.Json;

namespace Swashbuckle.Swagger20
{
    public class SwaggerObject
    {
        public string swagger = "2.0";

        public Info info;

        public string host;

        public string basePath;

        public IList<string> schemes;

        public IList<string> consumes;

        public IList<string> produces;

        public IDictionary<string, Path> paths;

        public IDictionary<string, Definition> definitions;

        public object security;

        public IList<Tag> tags;
    }

    public class Info
    {
        public string title;

        public string description;

        public string termsOfService;

        public Contact contact;

        public License license;

        public string version;
    }

    public class Contact
    {
        public string name;

        public string url;

        public string email;
    }

    public class License
    {
        public string name;

        public string url;
    }

    public class Path
    {
        [JsonProperty("$ref")]
        public string @ref;

        public Operation get;

        public Operation put;

        public Operation post;

        public Operation delete;

        public Operation options;

        public Operation head;

        public Operation patch;

        public IList<Parameter> parameters;
    }

    public class Operation
    {
        public IList<string> tags;

        public string summary;

        public string description;

        public ExternalDocumentation externalDocs;

        public string operationId;

        public IList<string> consumes;

        public IList<string> produces;

        public IList<Parameter> parameters;

        public IDictionary<string, Response> responses;

        public IList<string> schemes;

        public object security;
    }

    public class ExternalDocumentation
    {
        public string description;

        public string url;
    }

    public class Parameter : SerializableType
    {
        public string name;

        public string @in;

        public string description;

        public bool required;

        public Schema schema;
    }

    public class Schema
    {
        [JsonProperty("$ref")]
        public string @ref;
    }

    public class Response
    {
        public string description;

        public Schema schema;

        public SerializableType headers;

        public Example example;
    }

    public class SerializableType
    {
        public string type;

        public string format;

        public object items;

        public string collectionFormat;
    }

    public class Example
    {}

    public class Definition
    {}

    public class Tag
    {}
}