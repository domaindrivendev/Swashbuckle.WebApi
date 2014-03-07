using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;

namespace Swashbuckle.Models
{
    public class SwaggerGenerator
    {
        protected const string SwaggerVersion = "1.2";

        private readonly SwaggerSpecConfig _config;
        private readonly OperationSpecGenerator _operationSpecGenerator;

        public SwaggerGenerator()
            : this(SwaggerSpecConfig.StaticInstance)
        {}

        public SwaggerGenerator(SwaggerSpecConfig config)
        {
            _config = config;
            _operationSpecGenerator = new OperationSpecGenerator(
                config.CustomTypeMappings,
                config.SubTypesLookup,
                config.OperationFilters,
                config.OperationSpecFilters);
        }

        public SwaggerSpec ApiExplorerToSwaggerSpec(IApiExplorer apiExplorer)
        {
            var apiDescriptionGroups = apiExplorer.ApiDescriptions
                .Where(apiDesc => !_config.IgnoreObsoleteActions || !apiDesc.IsMarkedObsolete())
                .GroupBy(apiDesc => "/" + _config.DeclarationKeySelector(apiDesc))
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
                ApiVersion = _config.ApiVersion,
                SwaggerVersion = SwaggerVersion,
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
            var modelSpecRegistrar = new ModelSpecRegistrar();

            // Group further by relative path - each group corresponds to an ApiSpec
            var apiSpecs = apiDescriptionGroup
                .GroupBy(apiDesc => apiDesc.RelativePathSansQueryString())
                .Select(apiDescGrp => CreateApiSpec(apiDescGrp, modelSpecRegistrar))
                .OrderBy(apiSpec => apiSpec.Path)
                .ToList();

            return new ApiDeclaration
            {
                ApiVersion = _config.ApiVersion,
                SwaggerVersion = SwaggerVersion,
                BasePath = _config.BasePathResolver().TrimEnd('/'),
                ResourcePath = apiDescriptionGroup.Key,
                Apis = apiSpecs,
                Models = modelSpecRegistrar.ToDictionary()
            };
        }

        private ApiSpec CreateApiSpec(IGrouping<string, ApiDescription> apiDescriptionGroup, ModelSpecRegistrar modelSpecRegistrar)
        {
            var operationSpecs = apiDescriptionGroup
                .Select(apiDesc => _operationSpecGenerator.ApiDescriptionToOperationSpec(apiDesc, modelSpecRegistrar))
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
