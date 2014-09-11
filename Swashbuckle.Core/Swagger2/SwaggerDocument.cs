using System.Collections.Generic;
using Newtonsoft.Json;

namespace Swashbuckle.Swagger2
{
    public class SwaggerDocument : Extensible
    {
        public readonly string swagger = "2.0";

        public Info info;

        public ExternalDocs externalDocs;

        public string host;

        public string basePath;

        public IList<string> schemes;

        public IList<string> consumes;

        public IList<string> produces;

        public IDictionary<string, PathItem> paths;

        public IDictionary<string, Schema> definitions;

        public IDictionary<string, Parameter> parameters;

        public IList<object> security;

        public IList<Tag> tags;
    }

    public class Info
    {
        public string version;

        public string title;

        public string description;

        public string termsOfService;

        public Contact contact;

        public License license;
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

    public class ExternalDocs
    {
        public string description;

        public string url;
    }

    public class PathItem
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

        public ExternalDocs externalDocs;

        public string operationId;

        public IList<string> consumes;

        public IList<string> produces;

        public IList<Parameter> parameters;

        public IDictionary<string, Response> responses;

        public IList<string> schemes;

        public IList<string> security;
    }

    public class Parameter : SerializableType
    {
        public string name;

        public string @in;

        public string description;

        public bool required;

        public Schema schema;
    }

    public class Response
    {
        public string description;

        public Schema schema;

        public IList<SerializableType> headers;

        public object examples;
    }

    public class Schema
    {
        [JsonProperty("$ref")]
        public string @ref;

        public string format;

        public string title;

        public string description;

        public object @default;

        public object multipleOf;

        public object maximum;

        public bool? exclusiveMaximum;

        public object minimum;

        public bool? exclusiveMinimum;

        public int? maxLength;

        public int? minLength;

        public string pattern;

        public string discriminator;

        public Xml xml;

        public Schema items;

        public int? maxItems;

        public int? minItems;

        public bool? uniqueItems;

        public int? maxProperties;

        public int? minProperties;

        public IList<string> required;

        public ExternalDocs externalDocs;

        public IDictionary<string, Schema> definitions;

        public IDictionary<string, Schema> properties;

        public IList<string> @enum;

        public string type;

        public object example;

        public IList<Schema> allOf;
    }

    public class Xml
    {
        public string name;

        public string @namespace;

        public string prefix;

        public bool? attribute;

        public bool? wrapped;
    }

    public class SerializableType
    {
        public string type;

        public string format;

        public object items;

        public string collectionFormat;
    }

    public class Tag
    {
        public ExternalDocs externalDocs;
    }

    public class Extensible
    {
        [JsonExtensionData]
        public IDictionary<string, object> extensions = new Dictionary<string, object>();
    }
}