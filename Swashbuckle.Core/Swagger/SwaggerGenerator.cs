using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;

namespace Swashbuckle.Core.Swagger
{
    public class SwaggerGenerator
    {
        protected const string SwaggerVersion = "1.2";

        private readonly string _apiVersion;
        private readonly string _basePath;
        private readonly bool _ignoreObsoleteActions;
        private readonly Func<ApiDescription, string> _declarationKeySelector;

        private readonly OperationGenerator _operationGenerator;

        public SwaggerGenerator(
            string apiVersion,
            string basePath,
            bool ignoreObsoleteActions,
            Func<ApiDescription, string> declarationKeySelector,
            IEnumerable<IOperationFilter> operationFilters,
            IDictionary<Type, DataType> customTypeMappings,
            IEnumerable<PolymorphicType> polymorphicTypes)
        {
            _apiVersion = apiVersion;
            _basePath = basePath.TrimEnd('/');
            _ignoreObsoleteActions = ignoreObsoleteActions;
            _declarationKeySelector = declarationKeySelector;
            _operationGenerator = new OperationGenerator(operationFilters, customTypeMappings, polymorphicTypes);
        }

        public SwaggerSpec ApiExplorerToSwaggerSpec(IApiExplorer apiExplorer)
        {
            var apiDescriptionGroups = apiExplorer.ApiDescriptions
                .Where(apiDesc => !_ignoreObsoleteActions || !apiDesc.IsMarkedObsolete())
                .GroupBy(apiDesc => "/" + _declarationKeySelector(apiDesc))
                .OrderBy(group => group.Key)
                .ToArray();

            return new SwaggerSpec
                {
                    Listing = CreateListing(apiDescriptionGroups),
                    Declarations = CreateDeclarations(apiDescriptionGroups)
                };
        }

        private ResourceListing CreateListing(IEnumerable<IGrouping<string, ApiDescription>> apiDescriptionGroups)
        {
            var declarationLinks = apiDescriptionGroups
                .Select(apiDescGrp => new Resource { Path = apiDescGrp.Key })
                .ToArray();

            return new ResourceListing
            {
                SwaggerVersion = SwaggerVersion,
                ApiVersion = _apiVersion,
                Apis = declarationLinks
            };
        }

        private Dictionary<string, ApiDeclaration> CreateDeclarations(IEnumerable<IGrouping<string, ApiDescription>> apiDescriptionGroups)
        {
            return apiDescriptionGroups
                .ToDictionary(apiDescGrp => apiDescGrp.Key, CreateDeclaration);
        }

        private ApiDeclaration CreateDeclaration(IGrouping<string, ApiDescription> apiDescriptionGroup)
        {
            var complexModels = new Dictionary<string, DataType>();

            // Group further by relative path - each group corresponds to an Api
            var apis = apiDescriptionGroup
                .GroupBy(apiDesc => apiDesc.RelativePathSansQueryString())
                .Select(apiDescGrp => CreateApi(apiDescGrp, complexModels))
                .OrderBy(api => api.Path)
                .ToList();

            return new ApiDeclaration
            {
                SwaggerVersion = SwaggerVersion,
                ApiVersion = _apiVersion,
                BasePath = _basePath,
                ResourcePath = apiDescriptionGroup.Key,
                Apis = apis,
                Models = complexModels
            };
        }

        private Api CreateApi(IGrouping<string, ApiDescription> apiDescriptionGroup, Dictionary<string, DataType> complexModels)
        {
            var operations = apiDescriptionGroup
                .Select(apiDesc => _operationGenerator.ApiDescriptionToOperation(apiDesc, complexModels))
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
