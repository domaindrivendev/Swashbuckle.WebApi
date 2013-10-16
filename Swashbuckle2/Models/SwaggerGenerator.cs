using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Description;

namespace Swashbuckle.Models
{
    public class SwaggerGenerator
    {
        protected const string SwaggerVersion = "1.2";

        private readonly string _basePath;
        private readonly Func<ApiDescription, string> _declarationKeySelector;
        private readonly IEnumerable<IOperationSpecFilter> _operationSpecFilters;

        public SwaggerGenerator(
            string basePath,
            Func<ApiDescription, string> declarationKeySelector,
            IEnumerable<IOperationSpecFilter> operationSpecFilters)
        {
            _basePath = basePath;
            _declarationKeySelector = declarationKeySelector;
            _operationSpecFilters = operationSpecFilters;
        }

        public SwaggerSpec Generate(IApiExplorer apiExplorer)
        {
            var apiDescriptionGroups = apiExplorer.ApiDescriptions
                .Where(apiDesc => apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName != "ApiDocs") // Exclude the Swagger controller
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
            var modelSpecMap = new ModelSpecMap();

            // Group further by relative path - each group corresponds to an ApiSpec
            var apiSpecs = apiDescriptionGroup
                .GroupBy(apiDesc => apiDesc.RelativePath)
                .Select(apiDescGrp => GenerateApiSpec(apiDescGrp, modelSpecMap))
                .ToList();

            var complexModelSpecs = modelSpecMap.GetAll()
                .Where(modelSpec => modelSpec.Type == "object")
                .ToDictionary(modelSpec => modelSpec.Id, modelSpec => modelSpec);

            return new ApiDeclaration
            {
                ApiVersion = "1.0",
                SwaggerVersion = SwaggerVersion,
                BasePath = _basePath,
                ResourcePath = apiDescriptionGroup.Key,
                Apis = apiSpecs,
                Models = complexModelSpecs
            };
        }

        private ApiSpec GenerateApiSpec(IGrouping<string, ApiDescription> apiDescriptionGroup, ModelSpecMap modelSpecMap)
        {
            var operationSpecs = apiDescriptionGroup
                .Select(apiDesc => GenerateOperationSpec(apiDesc, modelSpecMap))
                .ToList();

            return new ApiSpec
            {
                Path = "/" + apiDescriptionGroup.Key.Split('?').First(),
                Operations = operationSpecs
            };
        }

        private OperationSpec GenerateOperationSpec(ApiDescription apiDescription, ModelSpecMap modelSpecMap)
        {
            var apiPath = apiDescription.RelativePath.Split('?').First();
            var paramSpecs = apiDescription.ParameterDescriptions
                .Select(paramDesc => GenerateParameterSpec(paramDesc, apiPath, modelSpecMap))
                .ToList();

            var operationSpec = new OperationSpec
            {
                Method = apiDescription.HttpMethod.Method,
                Nickname = String.Format("{0}_{1}",
                    apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName,
                    apiDescription.ActionDescriptor.ActionName),
                Summary = apiDescription.Documentation,
                Parameters = paramSpecs,
                ResponseMessages = new List<ResponseMessageSpec>()
            };

            var returnType = apiDescription.ActionDescriptor.ReturnType;
            if (returnType == null)
            {
                operationSpec.Type = "void";
            }
            else if (returnType != typeof(HttpResponseMessage))
            {
                var modelSpec = modelSpecMap.FindOrCreateFor(returnType);
                if (modelSpec.Type == "object")
                {
                    operationSpec.Type = modelSpec.Id;
                }
                else
                {
                    operationSpec.Type = modelSpec.Type;
                    operationSpec.Format = modelSpec.Format;
                    operationSpec.Items = modelSpec.Items;
                    operationSpec.Enum = modelSpec.Enum;
                }
            }

            foreach (var filter in _operationSpecFilters)
            {
                filter.Apply(apiDescription, operationSpec);
            }

            return operationSpec;
        }

        private ParameterSpec GenerateParameterSpec(ApiParameterDescription parameterDescription, string apiPath, ModelSpecMap modelSpecMap)
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

            var paramSpec = new ParameterSpec
            {
                ParamType = paramType,
                Name = parameterDescription.Name,
                Description = parameterDescription.Documentation,
                Required = !parameterDescription.ParameterDescriptor.IsOptional
            };

            var modelSpec = modelSpecMap.FindOrCreateFor(parameterDescription.ParameterDescriptor.ParameterType);
            if (modelSpec.Type == "object")
            {
                paramSpec.Type = modelSpec.Id;
            }
            else
            {
                paramSpec.Type = modelSpec.Type;
                paramSpec.Format = modelSpec.Format;
                paramSpec.Items = modelSpec.Items;
                paramSpec.Enum = modelSpec.Enum;
            }

            return paramSpec;
        }
    }
}
