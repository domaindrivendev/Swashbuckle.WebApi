using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;

namespace Swashbuckle.Models
{
    public class ApiExplorerAdapter : ISwaggerSpec
    {
        protected const string SwaggerVersion = "1.1";
        private readonly Lazy<Dictionary<string, ApiDeclaration>> _apiDeclarations;

        private readonly Func<string> _basePathAccessor;
        private readonly IEnumerable<IGrouping<string, ApiDescription>> _controllerGroups;
        private readonly IEnumerable<IOperationSpecFilter> _postFilters;
        private readonly Lazy<ResourceListing> _resourceListing;

        public ApiExplorerAdapter(
            IApiExplorer apiExplorer,
            Func<string> basePathAccessor,
            IEnumerable<IOperationSpecFilter> postFilters)
        {
            // Group ApiDescriptions by controller name - each group corresponds to an ApiDeclaration
            _controllerGroups = apiExplorer.ApiDescriptions
                .GroupBy(ad => "/swagger/api-docs/" + ad.ActionDescriptor.ControllerDescriptor.ControllerName);

            _basePathAccessor = basePathAccessor;
            _postFilters = postFilters;
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
            var declarationLinks = _controllerGroups
                .Select(dg => new ApiDeclarationLink {path = dg.Key})
                .ToList();

            return new ResourceListing
                {
                    apiVersion = "1.0",
                    swaggerVersion = SwaggerVersion,
                    basePath = _basePathAccessor(),
                    apis = declarationLinks
                };
        }

        private Dictionary<string, ApiDeclaration> GenerateApiDeclarations()
        {
            return _controllerGroups
                .ToDictionary(cg => cg.Key, DescriptionGroupToApiDeclaration);
        }

        private ApiDeclaration DescriptionGroupToApiDeclaration(IGrouping<string, ApiDescription> descriptionGroup)
        {
            var uniqueTypes = GetUniqueTypesForApis(descriptionGroup);
            var modelMap = new DataTypeMap(uniqueTypes);

            // Group further by relative path - each group corresponds to an ApiSpec
            var apiSpecs = descriptionGroup
                .GroupBy(ad => ad.RelativePath)
                .Select(dg => DescriptionGroupToApiSpec(dg, modelMap))
                .ToList();

            return new ApiDeclaration
                {
                    apiVersion = "1.0",
                    swaggerVersion = SwaggerVersion,
                    basePath = _basePathAccessor(),
                    resourcePath = descriptionGroup.Key,
                    apis = apiSpecs,
                    models = modelMap.ModelSpecs()
                };
        }

        private ApiSpec DescriptionGroupToApiSpec(IGrouping<string, ApiDescription> descriptionGroup, DataTypeMap dataTypeMap)
        {
            var pathParts = descriptionGroup.Key.Split('?');
            var pathOnly = pathParts[0];
            var queryString = pathParts.Length == 1 ? String.Empty : pathParts[1];

            var operationSpecs = descriptionGroup
                .Select(dg => DescriptionToOperationSpec(dg, queryString, dataTypeMap))
                .ToList();

            return new ApiSpec
                {
                    path = "/" + pathOnly,
                    description = String.Empty,
                    operations = operationSpecs
                };
        }

        private ApiOperationSpec DescriptionToOperationSpec(ApiDescription description, string queryString, DataTypeMap dataTypeMap)
        {
            var paramSpecs = description.ParameterDescriptions
                .Select(pd => ParamDescriptionToParameterSpec(pd, queryString.Contains(pd.Name), dataTypeMap))
                .ToList();

            var responseDataType = dataTypeMap.DataTypeFor(description.ActionDescriptor.ReturnType);

            var operationSpec = new ApiOperationSpec
                {
                    httpMethod = description.HttpMethod.Method,
                    nickname = description.ActionDescriptor.ControllerDescriptor.ControllerName,
                    parameters = paramSpecs,
                    responseClass = responseDataType,
                    summary = description.Documentation,
                    errorResponses = new List<ApiErrorResponseSpec>()
                };

            foreach (var filter in _postFilters)
            {
                filter.Apply(description, operationSpec);
            }

            return operationSpec;
        }

        private ApiParameterSpec ParamDescriptionToParameterSpec(ApiParameterDescription parameterDescription, bool isInQueryString, DataTypeMap dataTypeMap)
        {
            var paramType = "";
            switch (parameterDescription.Source)
            {
                case ApiParameterSource.FromBody:
                    paramType = "body";
                    break;
                case ApiParameterSource.FromUri:
                    paramType = isInQueryString ? "query" : "path";
                    break;
            }

            var dataType = dataTypeMap.DataTypeFor(parameterDescription.ParameterDescriptor.ParameterType);
            return new ApiParameterSpec
                {
                    paramType = paramType,
                    name = parameterDescription.Name,
                    description = parameterDescription.Documentation,
                    dataType = dataType,
                    required = !parameterDescription.ParameterDescriptor.IsOptional,
                    allowableValues = dataTypeMap.AllowableValuesFor(parameterDescription.ParameterDescriptor.ParameterType)
                };
        }

        private IEnumerable<Type> GetUniqueTypesForApis(IEnumerable<ApiDescription> apiDescriptions)
        {
            var arrayOfDescriptions = apiDescriptions.ToArray();

            var paramTypes = arrayOfDescriptions
                .SelectMany(d => d.ParameterDescriptions)
                .Select(pd => pd.ParameterDescriptor.ParameterType);

            var returnTypes = arrayOfDescriptions
                .Select(d => d.ActionDescriptor.ReturnType);

            return paramTypes
                .Union(returnTypes)
                .Distinct()
                .Where(t => t != null);
        }
    }
}