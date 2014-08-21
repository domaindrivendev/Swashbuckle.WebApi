using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;

namespace Swashbuckle.Swagger
{
    public class SwaggerGenerator
    {
        protected const string SwaggerVersion = "1.2";

        private readonly string _basePath;
        private readonly string _apiVersion;
        private readonly IEnumerable<ApiDescription> _apiDescriptions;
        private readonly SwaggerGeneratorOptions _options;

        public SwaggerGenerator(
            string basePath,
            string apiVersion,
            IEnumerable<ApiDescription> apiDescriptions,
            SwaggerGeneratorOptions options)
        {
            _basePath = basePath;
            _apiVersion = apiVersion;
            _apiDescriptions = apiDescriptions;
            _options = options;
        }

        public ResourceListing GetListing()
        {
            var resources = _apiDescriptions
                .GroupBy(apiDesc => _options.ResourceNameResolver(apiDesc))
                .OrderBy(group => group.Key, _options.ResourceNameComparer)
                .Select(apiDescGrp => new Resource { Path = "/" + apiDescGrp.Key })
                .ToArray();

            return new ResourceListing
            {
                SwaggerVersion = SwaggerVersion,
                ApiVersion = _apiVersion,
                Apis = resources
            };
        }

        public ApiDeclaration GetDeclaration(string resourceName)
        {
            var apiDescriptionGroup = _apiDescriptions
                .GroupBy(apiDesc => _options.ResourceNameResolver(apiDesc))
                .Single(apiDescGrp => apiDescGrp.Key == resourceName);

            var dataTypeRegistry = new DataTypeRegistry(
                _options.CustomTypeMappings,
                _options.PolymorphicTypes,
                _options.ModelFilters);

            var operationGenerator = new OperationGenerator(
                dataTypeRegistry,
                _options.OperationFilters);

            // Group further by relative path - each group corresponds to an Api
            var apis = apiDescriptionGroup
                .GroupBy(apiDesc => apiDesc.RelativePathSansQueryString())
                .Select(apiDescGrp => CreateApi(apiDescGrp, operationGenerator))
                .OrderBy(api => api.Path)
                .ToList();

            return new ApiDeclaration
            {
                SwaggerVersion = SwaggerVersion,
                ApiVersion = _apiVersion,
                BasePath = _basePath,
                ResourcePath = "/" + resourceName,
                Apis = apis,
                Models = dataTypeRegistry.GetModels()
            };
        }

        private Api CreateApi(IGrouping<string, ApiDescription> apiDescriptionGroup, OperationGenerator operationGenerator)
        {
            var operations = apiDescriptionGroup
                .Select(operationGenerator.ApiDescriptionToOperation)
                .OrderBy(op => op.Method)
                .ToList();

            return new Api
            {
                Path = "/" + apiDescriptionGroup.Key,
                Operations = operations
            };
        }
    }
}