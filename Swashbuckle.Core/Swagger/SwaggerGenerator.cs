using System.Linq;
using System.Collections.Generic;
using System.Web.Http.Description;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.Swagger
{
    public class SwaggerGenerator : ISwaggerProvider
    {
        private readonly string _rootUrl;
        private readonly IApiExplorer _apiExplorer;
        private readonly IContractResolver _jsonContractResolver;
        private readonly SwaggerGeneratorSettings _settings;

        public SwaggerGenerator(
            string rootUrl,
            IApiExplorer apiExplorer,
            IContractResolver jsonContractResolver,
            SwaggerGeneratorSettings settings)
        {
            _rootUrl = rootUrl;
            _apiExplorer = apiExplorer;
            _jsonContractResolver = jsonContractResolver;
            _settings = settings;
        }

        public SwaggerDocument GetSwaggerFor(string apiVersion)
        {
            var schemaRegistry = new SchemaRegistry(_jsonContractResolver, _settings.CustomSchemaMappings, _settings.SchemaFilters);

            Info info;
            _settings.ApiVersions.TryGetValue(apiVersion, out info);
            if (info == null)
                throw new UnknownApiVersion(apiVersion);

            var paths = GetApiDescriptionsFor(apiVersion)
                .OrderBy(_settings.GroupingKeySelector, _settings.GroupingKeyComparer)
                .GroupBy(apiDesc => apiDesc.RelativePathSansQueryString())
                .ToDictionary(group => "/" + group.Key, group => CreatePathItem(group, schemaRegistry));

            var rootUrl = new Uri(_rootUrl);

            var swaggerDoc = new SwaggerDocument
            {
                info = info,
                host = rootUrl.Host + ":" + rootUrl.Port,
                basePath = (rootUrl.AbsolutePath != "/") ? rootUrl.AbsolutePath : null,
                schemes = (_settings.Schemes != null) ? _settings.Schemes.ToList() : new[] { rootUrl.Scheme }.ToList(),
                paths = paths,
                definitions = schemaRegistry.Definitions,
                securityDefinitions = _settings.SecurityDefinitions
            };

            foreach(var filter in _settings.DocumentFilters)
            {
                filter.Apply(swaggerDoc, schemaRegistry, _apiExplorer);
            }

            return swaggerDoc;
        }

        private IEnumerable<ApiDescription> GetApiDescriptionsFor(string apiVersion)
        {
            return _apiExplorer.ApiDescriptions
                .Where(apiDesc => _settings.VersionSupportResolver(apiDesc, apiVersion));
        }

        private PathItem CreatePathItem(IEnumerable<ApiDescription> apiDescriptions, SchemaRegistry schemaRegistry)
        {
            var pathItem = new PathItem();

            // Group further by http method
            var perMethodGrouping = apiDescriptions
                .GroupBy(apiDesc => apiDesc.HttpMethod.Method.ToLower());

            foreach (var group in perMethodGrouping)
            {
                var httpMethod = group.Key;

                var apiDescription = (group.Count() == 1)
                    ? group.First()
                    : _settings.ConflictingActionsResolver(group);

                switch (httpMethod)
                {
                    case "get":
                        pathItem.get = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "put":
                        pathItem.put = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "post":
                        pathItem.post = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "delete":
                        pathItem.delete = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "options":
                        pathItem.options = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "head":
                        pathItem.head = CreateOperation(apiDescription, schemaRegistry);
                        break;
                    case "patch":
                        pathItem.patch = CreateOperation(apiDescription, schemaRegistry);
                        break;
                }
            }

            return pathItem;
        }

        private Operation CreateOperation(ApiDescription apiDescription, SchemaRegistry schemaRegistry)
        {
            var parameters = apiDescription.ParameterDescriptions
                .Select(paramDesc =>
                    {
                        var inPath = apiDescription.RelativePathSansQueryString().Contains("{" + paramDesc.Name + "}");
                        return CreateParameter(paramDesc, inPath, schemaRegistry);
                    })
                 .ToList();

            // TODO: Always 200 - is this a little presumptious?
            var responses = new Dictionary<string, Response>{
                { "200", CreateResponse(apiDescription.SuccessResponseType(), schemaRegistry) }
            };

            var operation = new Operation
            { 
                tags = new [] { _settings.GroupingKeySelector(apiDescription) },
                operationId = apiDescription.FriendlyId(),
                produces = apiDescription.Produces().ToList(),
                consumes = apiDescription.Consumes().ToList(),
                parameters = parameters.Any() ? parameters : null, // parameters can be null but not empty
                responses = responses,
                deprecated = apiDescription.IsObsolete()
            };

            foreach (var filter in _settings.OperationFilters)
            {
                filter.Apply(operation, schemaRegistry, apiDescription);
            }

            return operation;
        }

        private Parameter CreateParameter(ApiParameterDescription paramDesc, bool inPath, SchemaRegistry schemaRegistry)
        {
            var @in = (inPath)
                ? "path"
                : (paramDesc.Source == ApiParameterSource.FromUri) ? "query" : "body";

            var parameter = new Parameter
            {
                name = paramDesc.Name,
                @in = @in
            };

            if (paramDesc.ParameterDescriptor == null)
            {
                parameter.type = "string";
                parameter.required = true;
                return parameter; 
            }

            parameter.required = !paramDesc.ParameterDescriptor.IsOptional;

            var schema = schemaRegistry.FindOrRegister(paramDesc.ParameterDescriptor.ParameterType);
            if (parameter.@in == "body")
                parameter.schema = schema;
            else
                parameter.PopulateFrom(schema);

            return parameter;
        }

        private Response CreateResponse(Type returnType, SchemaRegistry schemaRegistry)
        {
            var schema = (returnType != null)
                ? schemaRegistry.FindOrRegister(returnType)
                : null;

            return new Response
            {
                description = "OK",
                schema = schema
            };
        }
    }
}