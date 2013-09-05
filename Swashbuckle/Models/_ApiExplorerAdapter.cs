//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Collections.Specialized;
//using System.Linq;
//using System.Web.Http.Description;
//using Newtonsoft.Json.Schema;
//
//namespace Swashbuckle.Models
//{
//    public class ApiExplorerAdapter : ISwaggerSpec
//    {
//        protected const string SwaggerVersion = "1.2";
//        private readonly Lazy<Dictionary<string, ApiDeclaration>> _apiDeclarations;
//
//        private readonly Func<string> _basePathAccessor;
//        private readonly IEnumerable<IGrouping<string, ApiDescription>> _controllerGroups;
//        private readonly IEnumerable<IOperationSpecFilter> _postFilters;
//        private readonly Lazy<ResourceListing> _resourceListing;
//        private readonly JsonSchemaGenerator _jsonSchemaGenerator;
//
//        public ApiExplorerAdapter(
//            IApiExplorer apiExplorer,
//            Func<string> basePathAccessor,
//            IEnumerable<IOperationSpecFilter> postFilters)
//        {
//            // Group ApiDescriptions by controller name - each group corresponds to an ApiDeclaration
//            _controllerGroups = apiExplorer.ApiDescriptions
//                .GroupBy(ad => "/swagger/api-docs/" + ad.ActionDescriptor.ControllerDescriptor.ControllerName);
//
//            _basePathAccessor = basePathAccessor;
//            _postFilters = postFilters;
//            _resourceListing = new Lazy<ResourceListing>(GenerateResourceListing);
//            _apiDeclarations = new Lazy<Dictionary<string, ApiDeclaration>>(GenerateApiDeclarations);
//
//            _jsonSchemaGenerator = new JsonSchemaGenerator { UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName };
//        }
//
//        public ResourceListing GetResourceListing()
//        {
//            return _resourceListing.Value;
//        }
//
//        public ApiDeclaration GetApiDeclaration(string resourcePath)
//        {
//            return _apiDeclarations.Value[resourcePath];
//        }
//
//        private ResourceListing GenerateResourceListing()
//        {
//            var declarationLinks = _controllerGroups
//                .Select(dg => new ApiDeclarationLink {path = dg.Key})
//                .ToList();
//
//            return new ResourceListing
//                {
//                    apiVersion = "1.0",
//                    swaggerVersion = SwaggerVersion,
//                    apis = declarationLinks
//                };
//        }
//
//        private Dictionary<string, ApiDeclaration> GenerateApiDeclarations()
//        {
//            return _controllerGroups
//                .ToDictionary(cg => cg.Key, DescriptionGroupToApiDeclaration);
//        }
//
//        private ApiDeclaration DescriptionGroupToApiDeclaration(IGrouping<string, ApiDescription> descriptionGroup)
//        {
//            var modelSpecsBuilder = new ModelSpecsBuilder(_jsonSchemaGenerator);
//
//            // Group further by relative path - each group corresponds to an ApiSpec
//            var apiSpecs = descriptionGroup
//                .GroupBy(ad => ad.RelativePath)
//                .Select(dg => DescriptionGroupToApiSpec(dg, modelSpecsBuilder))
//                .ToList();
//
//            return new ApiDeclaration
//                {
//                    apiVersion = "1.0",
//                    swaggerVersion = SwaggerVersion,
//                    basePath = _basePathAccessor(),
//                    resourcePath = descriptionGroup.Key,
//                    apis = apiSpecs,
//                    //models = modelSpecsBuilder.Build()
//                };
//        }
//
//        private ApiSpec DescriptionGroupToApiSpec(IGrouping<string, ApiDescription> descriptionGroup, ModelSpecsBuilder modelSpecsBuilder)
//        {
//            var pathParts = descriptionGroup.Key.Split('?');
//            var pathOnly = pathParts[0];
//            var queryString = pathParts.Length == 1 ? String.Empty : pathParts[1];
//
//            var operationSpecs = descriptionGroup
//                .Select(dg => DescriptionToOperationSpec(dg, queryString, modelSpecsBuilder))
//                .ToList();
//
//            return new ApiSpec
//                {
//                    path = "/" + pathOnly,
//                    description = String.Empty,
//                    operations = operationSpecs
//                };
//        }
//
//        private ApiOperationSpec DescriptionToOperationSpec(ApiDescription description, string queryString, ModelSpecsBuilder modelSpecsBuilder)
//        {
//            modelSpecsBuilder.AddType(description.ActionDescriptor.ReturnType);
//
//            var paramSpecs = description.ParameterDescriptions
//                .Select(pd => ParamDescriptionToParameterSpec(pd, queryString.Contains(pd.Name), modelSpecsBuilder))
//                .ToList();
//
//
//            var responseSchema = _jsonSchemaGenerator.Generate(description.ActionDescriptor.ReturnType);
//            var operationSpec = new ApiOperationSpec
//                {
//                    httpMethod = description.HttpMethod.Method,
//                    nickname = description.ActionDescriptor.ControllerDescriptor.ControllerName,
//                    parameters = paramSpecs,
//                    summary = description.Documentation,
//                    responseMessages = new List<ApiResponseMessageSpec>()
//                };
//            SetValuesFromJsonSchema(operationSpec, responseSchema);
//
//            foreach (var filter in _postFilters)
//            {
//                filter.Apply(description, operationSpec);
//            }
//
//            return operationSpec;
//        }
//
//        private ApiParameterSpec ParamDescriptionToParameterSpec(ApiParameterDescription parameterDescription, bool isInQueryString, ModelSpecsBuilder modelSpecsBuilder)
//        {
//            modelSpecsBuilder.AddType(parameterDescription.ParameterDescriptor.ParameterType);
//
//            var paramType = "";
//            switch (parameterDescription.Source)
//            {
//                case ApiParameterSource.FromBody:
//                    paramType = "body";
//                    break;
//                case ApiParameterSource.FromUri:
//                    paramType = isInQueryString ? "query" : "path";
//                    break;
//            }
//
//            var swaggerType = parameterDescription.ParameterDescriptor.ParameterType.ToSwaggerType();
//            return new ApiParameterSpec
//                {
//                    paramType = paramType,
//                    name = parameterDescription.Name,
//                    description = parameterDescription.Documentation,
//                    type = swaggerType.Name,
//                    format = swaggerType.Format,
//                    @enum = swaggerType.Enum,
//                    required = !parameterDescription.ParameterDescriptor.IsOptional
//                };
//        }
//
//        private void SetValuesFromJsonSchema(JsonSchemaSpec spec, JsonSchema schema)
//        {
//            if (schema.Id != null)
//                spec.id = schema.Id.Split('.').Last();
//
//            spec.type = (schema.Type == JsonSchemaType.Object) ? spec.id : schema.Type.ToString();
//            spec.format = schema.Format;
//            //spec.@enum = schema.Enum.Select(jToken => jToken.First);
//        }
//    }
//}