using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;

namespace Swashbuckle.Models
{
    public class SwaggerGenerator
    {
        protected const string SwaggerVersion = "1.2";

        static SwaggerGenerator()
        {
            Instance = new SwaggerGenerator(
                SwaggerSpecConfig.Instance.DeclarationKeySelector,
                SwaggerSpecConfig.Instance.BasePathResolver,
                SwaggerSpecConfig.Instance.OperationSpecFilters);
        }

        public static SwaggerGenerator Instance { get; private set; }

        private readonly Func<ApiDescription, string> _declarationKeySelector;
        private readonly Func<string> _basePathResolver;
        private readonly IEnumerable<IOperationSpecFilter> _operationSpecFilters;

        private SwaggerGenerator(
            Func<ApiDescription, string> declarationKeySelector,
            Func<string> basePathResolver,
            IEnumerable<IOperationSpecFilter> operationSpecFilters)
        {
            _declarationKeySelector = declarationKeySelector;
            _basePathResolver = basePathResolver;
            _operationSpecFilters = operationSpecFilters;
        }

        public SwaggerSpec Generate(IApiExplorer apiExplorer)
        {
            var apiDescriptionGroups = apiExplorer.ApiDescriptions
                .GroupBy(apiDesc => "/" + _declarationKeySelector(apiDesc))
                .ToArray();

            return new SwaggerSpec
                {
                    Listing = GenerateListing(apiDescriptionGroups),
                    Declarations = GenerateDeclarations(apiDescriptionGroups)
                };
        }

        private ResourceListing GenerateListing(IEnumerable<IGrouping<string, ApiDescription>> apiDescriptionGroups)
        {
            var declarationLinks = apiDescriptionGroups
                .Select(apiDescGrp => new ApiDeclarationLink { Path = apiDescGrp.Key })
                .ToArray();

            return new ResourceListing
            {
                ApiVersion = "1.0",
                SwaggerVersion = SwaggerVersion,
                Apis = declarationLinks
            };
        }

        private Dictionary<string, ApiDeclaration> GenerateDeclarations(IEnumerable<IGrouping<string, ApiDescription>> apiDescriptionGroups)
        {
            return apiDescriptionGroups
                .ToDictionary(apiDescGrp => apiDescGrp.Key, GenerateDeclaration);
        }

        private ApiDeclaration GenerateDeclaration(IGrouping<string, ApiDescription> apiDescriptionGroup)
        {
            // Group further by relative path - each group corresponds to an ApiSpec
            var apiSpecs = apiDescriptionGroup
                .GroupBy(apiDesc => apiDesc.RelativePath)
                .Select(GenerateApiSpec)
                .ToList();

            return new ApiDeclaration
            {
                ApiVersion = "1.0",
                SwaggerVersion = SwaggerVersion,
                BasePath = _basePathResolver(),
                ResourcePath = apiDescriptionGroup.Key,
                Apis = apiSpecs,
                //Models = modelSpecsBuilder.Build()
            };
        }

        private ApiSpec GenerateApiSpec(IGrouping<string, ApiDescription> apiDescriptionGroup)
        {
            var operationSpecs = apiDescriptionGroup
                .Select(GenerateOperationSpec)
                .ToList();

            return new ApiSpec
            {
                Path = "/" + apiDescriptionGroup.Key.Split('?').First(),
                Operations = operationSpecs
            };
        }

        private OperationSpec GenerateOperationSpec(ApiDescription apiDescription)
        {
            var apiPath = apiDescription.RelativePath.Split('?').First();
            var paramSpecs = apiDescription.ParameterDescriptions
                .Select(paramDesc => GenerateParameterSpec(paramDesc, apiPath))
                .ToList();

            var operationSpec = new OperationSpec
            {
                Method = apiDescription.HttpMethod.Method,
                Nickname = String.Format("{0}_{1}",
                    apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName,
                    apiDescription.ActionDescriptor.ActionName),
                Summary = apiDescription.Documentation,
                Type = apiDescription.ActionDescriptor.ReturnType.ToSwaggerType(),
                Parameters = paramSpecs,
                ResponseMessages = new List<ResponseMessageSpec>()
            };

            foreach (var filter in _operationSpecFilters)
            {
                filter.Apply(apiDescription, operationSpec);
            }

            return operationSpec;
        }

        private ParameterSpec GenerateParameterSpec(ApiParameterDescription parameterDescription, string apiPath)
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

            return new ParameterSpec
            {
                ParamType = paramType,
                Name = parameterDescription.Name,
                Description = parameterDescription.Documentation,
                Required = !parameterDescription.ParameterDescriptor.IsOptional,
                Type = parameterDescription.ParameterDescriptor.ParameterType.ToSwaggerType(),
            };
        }
    }
}
