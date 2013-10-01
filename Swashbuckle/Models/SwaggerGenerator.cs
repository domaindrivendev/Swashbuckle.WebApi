using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;

namespace Swashbuckle.Models
{
    public class SwaggerGenerator
    {
        protected const string SwaggerVersion = "1.1";

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
                .GroupBy(apiDesc => "/swagger/api-docs/" + _declarationKeySelector(apiDesc))
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
                .Select(apiDescGrp => new ApiDeclarationLink { path = apiDescGrp.Key })
                .ToArray();

            return new ResourceListing
            {
                apiVersion = "1.0",
                swaggerVersion = SwaggerVersion,
                basePath = _basePathResolver(),
                apis = declarationLinks
            };
        }

        private Dictionary<string, ApiDeclaration> GenerateDeclarations(IEnumerable<IGrouping<string, ApiDescription>> apiDescriptionGroups)
        {
            return apiDescriptionGroups
                .ToDictionary(apiDescGrp => apiDescGrp.Key, GenerateDeclaration);
        }

        private ApiDeclaration GenerateDeclaration(IGrouping<string, ApiDescription> apiDescriptionGroup)
        {
            var modelSpecsBuilder = new ModelSpecsBuilder();

            // Group further by relative path - each group corresponds to an ApiSpec
            var apiSpecs = apiDescriptionGroup
                .GroupBy(apiDesc => apiDesc.RelativePath)
                .Select(apiDescGrp => GenerateApiSpec(apiDescGrp, modelSpecsBuilder))
                .ToList();

            return new ApiDeclaration
            {
                apiVersion = "1.0",
                swaggerVersion = SwaggerVersion,
                basePath = _basePathResolver(),
                resourcePath = apiDescriptionGroup.Key,
                apis = apiSpecs,
                models = modelSpecsBuilder.Build()
            };
        }

        private ApiSpec GenerateApiSpec(IGrouping<string, ApiDescription> apiDescriptionGroup, ModelSpecsBuilder modelSpecsBuilder)
        {
            var operationSpecs = apiDescriptionGroup
                .Select(apiDesc => GenerateOperationSpec(apiDesc, modelSpecsBuilder))
                .ToList();

            return new ApiSpec
            {
                path = "/" + apiDescriptionGroup.Key.Split('?').First(),
                operations = operationSpecs
            };
        }

        private ApiOperationSpec GenerateOperationSpec(ApiDescription apiDescription, ModelSpecsBuilder modelSpecsBuilder)
        {
            modelSpecsBuilder.AddType(apiDescription.ActionDescriptor.ReturnType);


            var apiPath = apiDescription.RelativePath.Split('?').First();
            var paramSpecs = apiDescription.ParameterDescriptions
                .Select(paramDesc => GenerateParameterSpec(paramDesc, apiPath, modelSpecsBuilder))
                .ToList();

            var operationSpec = new ApiOperationSpec
            {
                httpMethod = apiDescription.HttpMethod.Method,
                nickname = String.Format("{0}_{1}",
                    apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName,
                    apiDescription.ActionDescriptor.ActionName),
                parameters = paramSpecs,
                responseClass = apiDescription.ActionDescriptor.ReturnType.ToSwaggerType(),
                summary = apiDescription.Documentation,
                errorResponses = new List<ApiErrorResponseSpec>()
            };

            foreach (var filter in _operationSpecFilters)
            {
                filter.Apply(apiDescription, operationSpec);
            }

            return operationSpec;
        }

        private ApiParameterSpec GenerateParameterSpec(ApiParameterDescription parameterDescription, string apiPath, ModelSpecsBuilder modelSpecsBuilder)
        {
            modelSpecsBuilder.AddType(parameterDescription.ParameterDescriptor.ParameterType);

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

            return new ApiParameterSpec
            {
                paramType = paramType,
                name = parameterDescription.Name,
                description = parameterDescription.Documentation,
                dataType = parameterDescription.ParameterDescriptor.ParameterType.ToSwaggerType(),
                required = !parameterDescription.ParameterDescriptor.IsOptional,
                allowableValues = parameterDescription.ParameterDescriptor.ParameterType.AllowableValues()
            };
        }
    }
}
