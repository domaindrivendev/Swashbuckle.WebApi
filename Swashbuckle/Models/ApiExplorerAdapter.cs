using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Description;
using Newtonsoft.Json.Schema;

namespace Swashbuckle.Models
{
    public class ApiExplorerAdapter : ISwaggerSpec
    {
        protected const string SwaggerVersion = "1.2";

        private readonly IEnumerable<IGrouping<string, ApiDescription>> _apiGroups;
        private readonly Func<string> _basePathAccessor;
        private readonly IEnumerable<IOperationSpecFilter> _postFilters;
        private readonly JsonSchemaGenerator _jsonSchemaGenerator;
        private readonly Lazy<Dictionary<string, ApiDeclaration>> _apiDeclarations;
        private readonly Lazy<ResourceListing> _resourceListing;

        public ApiExplorerAdapter(
            IApiExplorer apiExplorer,
            IGroupingStrategy groupingStrategy,
            IEnumerable<IOperationSpecFilter> postFilters,
            Func<string> basePathAccessor)
        {
            // Initial grouping - Api Declaration for each group
            _apiGroups = apiExplorer.ApiDescriptions.GroupBy(description => "/swagger/api-docs/" + groupingStrategy.GetKeyFrom(description));

            _postFilters = postFilters;
            _basePathAccessor = basePathAccessor;
            _jsonSchemaGenerator = new JsonSchemaGenerator {UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName};

            _resourceListing = new Lazy<ResourceListing>(GenerateResourceListing);
            _apiDeclarations = new Lazy<Dictionary<string, ApiDeclaration>>(GenerateApiDeclarations);
        }

        public ResourceListing GetResourceListing()
        {
            return _resourceListing.Value;
        }

        public ApiDeclaration GetApiDeclaration(string resourcePath)
        {
            return _apiDeclarations.Value[resourcePath];
        }

        private ResourceListing GenerateResourceListing()
        {
            return new ResourceListing
                {
                    apiVersion = "1.0",
                    swaggerVersion = SwaggerVersion,
                    apis = _apiGroups.Select(group => new ApiDeclarationLink {path = group.Key}).ToArray()
                };
        }

        private Dictionary<string, ApiDeclaration> GenerateApiDeclarations()
        {
            return _apiGroups
                .ToDictionary(group => group.Key, DescriptionGroupToApiDeclaration);
        }

        private ApiDeclaration DescriptionGroupToApiDeclaration(IGrouping<string, ApiDescription> descriptionGroup)
        {
            var modelSpecs = new Dictionary<string, ModelSpec>();

            // Group further by relative path - ApiSpec for each group
            var apiSpecs = descriptionGroup
                .GroupBy(ad => "/" + ad.RelativePath)
                .Select(group => DescriptionGroupToApiSpec(group, modelSpecs))
                .ToArray();

            return new ApiDeclaration
                {
                    apiVersion = "1.0",
                    swaggerVersion = SwaggerVersion,
                    basePath = _basePathAccessor(),
                    resourcePath = descriptionGroup.Key,
                    apis = apiSpecs,
                    models = modelSpecs
                };
        }

        private ApiSpec DescriptionGroupToApiSpec(IGrouping<string, ApiDescription> descriptionGroup, IDictionary<string, ModelSpec> modelSpecs)
        {
            var operationSpecs = descriptionGroup
                .Select(group => DescriptionToOperationSpec(group, modelSpecs))
                .ToArray();

            return new ApiSpec
                {
                    path = descriptionGroup.Key.Split('?').First(),
                    operations = operationSpecs
                };
        }

        private OperationSpec DescriptionToOperationSpec(ApiDescription description, IDictionary<string, ModelSpec> modelSpecs)
        {
            var apiPath = description.RelativePath.Split('?').First();

            var paramSpecs = description.ParameterDescriptions
                .Select(pd => ParamDescriptionToParameterSpec(pd, apiPath, modelSpecs))
                .ToArray();

            var operationSpec = new OperationSpec
                {
                    method = description.HttpMethod.Method,
                    nickname = String.Format("{0}_{1}",
                        description.ActionDescriptor.ControllerDescriptor.ControllerName,
                        description.ActionDescriptor.ActionName),
                    parameters = paramSpecs,
                    summary = description.Documentation,
                    responseMessages = new List<ResponseMessageSpec>()
                };

            var returnType = description.ActionDescriptor.ReturnType;
            if (returnType == null)
            {
                operationSpec.type = "void";
            }
            else if (returnType != typeof (HttpResponseMessage))
            {
                var jsonSchema = _jsonSchemaGenerator.Generate(returnType);
                var complexJsonSchemas = _jsonSchemaNormalizer(jsonSchema);


                operationSpec.type = modelSpec.type;
                operationSpec.items = modelSpec.items;
                operationSpec.@enum = modelSpec.@enum;
            }

            foreach (var filter in _postFilters)
            {
                filter.Apply(description, operationSpec);
            }

            return operationSpec;
        }

        private ParameterSpec ParamDescriptionToParameterSpec(
            ApiParameterDescription parameterDescription,
            string apiPath,
            IDictionary<string, ModelSpec> modelSpecs)
        {
            var paramType = "";
            switch (parameterDescription.Source)
            {
                case ApiParameterSource.FromBody:
                    paramType = "body";
                    break;
                case ApiParameterSource.FromUri:
                    paramType = apiPath.Contains(parameterDescription.Name) ? "path" : "query";
                    break;
            }

            var schema = _jsonSchemaGenerator.Generate(parameterDescription.ParameterDescriptor.ParameterType);
            if (schema.Type == JsonSchemaType.Object)
                jsonSchemas.Add(schema);

            var modelSpec = JsonSchemaToModelSpec(schema);

            return new ParameterSpec
                {
                    paramType = paramType,
                    name = parameterDescription.Name,
                    description = parameterDescription.Documentation,
                    required = !parameterDescription.ParameterDescriptor.IsOptional,
                    type = modelSpec.type,
                    items = modelSpec.items,
                    @enum = modelSpec.@enum
                };
        }
    }
}