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

        private readonly OperationSpecGenerator _operationSpecGenerator;

        public SwaggerGenerator(
            string apiVersion,
            string basePath,
            bool ignoreObsoleteActions,
            Func<ApiDescription, string> declarationKeySelector,
            Dictionary<Type, ModelSpec> customTypeMappings,
            IEnumerable<PolymorphicType> polymorphicTypes,
            IEnumerable<IOperationSpecFilter> operationSpecFilters)
        {
            _apiVersion = apiVersion;
            _basePath = basePath.TrimEnd('/');
            _ignoreObsoleteActions = ignoreObsoleteActions;
            _declarationKeySelector = declarationKeySelector;
            _operationSpecGenerator = new OperationSpecGenerator(customTypeMappings, polymorphicTypes, operationSpecFilters);
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
                .Select(apiDescGrp => new ApiDeclarationLink { Path = apiDescGrp.Key })
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
            var complexModels = new Dictionary<string, ModelSpec>();

            // Group further by relative path - each group corresponds to an ApiSpec
            var apiSpecs = apiDescriptionGroup
                .GroupBy(apiDesc => apiDesc.RelativePathSansQueryString())
                .Select(apiDescGrp => CreateApiSpec(apiDescGrp, complexModels))
                .OrderBy(apiSpec => apiSpec.Path)
                .ToList();

            return new ApiDeclaration
            {
                SwaggerVersion = SwaggerVersion,
                ApiVersion = _apiVersion,
                BasePath = _basePath,
                ResourcePath = apiDescriptionGroup.Key,
                Apis = apiSpecs,
                Models = complexModels
            };
        }

        private ApiSpec CreateApiSpec(IGrouping<string, ApiDescription> apiDescriptionGroup, Dictionary<string, ModelSpec> complexModels)
        {
            var operationSpecs = apiDescriptionGroup
                .Select(apiDesc => _operationSpecGenerator.ApiDescriptionToOperationSpec(apiDesc, complexModels))
                .OrderBy(operationSpec => operationSpec.Method)
                .ToList();

            return new ApiSpec
            {
                Path = "/" + apiDescriptionGroup.Key,
                Operations = operationSpecs
            };
        }
    }
}
