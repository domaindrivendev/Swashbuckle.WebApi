using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;

namespace Swashbuckle.Swagger
{
    public class ApiExplorerAdapter : ISwaggerProvider
    {
        protected const string SwaggerVersion = "1.2";

        private readonly IApiExplorer _apiExplorer;
        private readonly bool _ignoreObsoleteActions;
        private readonly Func<ApiDescription, string, bool> _resoveVersionSupport;
        private readonly Func<ApiDescription, string> _resolveResourceName;
        private readonly IEnumerable<PolymorphicType> _polymorphicTypes;
        private readonly IEnumerable<IModelFilter> _modelFilters;
        private readonly IEnumerable<IOperationFilter> _operationFilters;

        public ApiExplorerAdapter(
            IApiExplorer apiExplorer,
            bool ignoreObsoleteActions,
            Func<ApiDescription, string, bool> resoveVersionSupport,
            Func<ApiDescription, string> resolveResourceName,
            IEnumerable<PolymorphicType> polymorphicTypes,
            IEnumerable<IModelFilter> modelFilters,
            IEnumerable<IOperationFilter> operationFilters)
        {
            _apiExplorer = apiExplorer;
            _ignoreObsoleteActions = ignoreObsoleteActions;
            _resoveVersionSupport = resoveVersionSupport;
            _resolveResourceName = resolveResourceName;
            _polymorphicTypes = polymorphicTypes;
            _modelFilters = modelFilters;
            _operationFilters = operationFilters;
        }

        public ResourceListing GetListing(string basePath, string version)
        {
            var apiDescriptionGroups = GetApplicableApiDescriptions(version);

            var resources = apiDescriptionGroups
                .Select(apiDescGrp => new Resource { Path = "/" + apiDescGrp.Key })
                .ToArray();

            return new ResourceListing
            {
                SwaggerVersion = SwaggerVersion,
                ApiVersion = version,
                Apis = resources
            };
        }

        public ApiDeclaration GetDeclaration(string basePath, string version, string resourceName)
        {
            var apiDescriptionGroup = GetApplicableApiDescriptions(version)
                .Single(apiDescGrp => apiDescGrp.Key == resourceName);

            var dataTypeRegistry = new DataTypeRegistry(_polymorphicTypes, _modelFilters);
            var operationGenerator = new OperationGenerator(dataTypeRegistry, _operationFilters);

            // Group further by relative path - each group corresponds to an Api
            var apis = apiDescriptionGroup
                .GroupBy(apiDesc => apiDesc.RelativePathSansQueryString())
                .Select(apiDescGrp => CreateApi(apiDescGrp, operationGenerator))
                .OrderBy(api => api.Path)
                .ToList();

            return new ApiDeclaration
            {
                SwaggerVersion = SwaggerVersion,
                ApiVersion = version,
                BasePath = basePath,
                ResourcePath = "/" + resourceName,
                Apis = apis,
                Models = dataTypeRegistry.GetModels()
            };
        }

        private IEnumerable<IGrouping<string, ApiDescription>> GetApplicableApiDescriptions(string version)
        {
            return _apiExplorer.ApiDescriptions
                .Where(apiDesc => !_ignoreObsoleteActions || !apiDesc.IsMarkedObsolete())
                .Where(apiDesc => _resoveVersionSupport(apiDesc, version))
                .GroupBy(apiDesc => _resolveResourceName(apiDesc))
                .OrderBy(group => group.Key)
                .ToArray();
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