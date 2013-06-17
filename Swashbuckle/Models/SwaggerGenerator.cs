using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Web.Http.Description;

namespace Swashbuckle.Models
{
    public class SwaggerGenerator
    {
        private const string SwaggerVersion = "1.1";

        private static readonly StringDictionary PrimtiveTypeMap = new StringDictionary
            {
                {"Byte", "byte"},
                {"Boolean", "boolean"},
                {"Int32", "int"},
                {"Int64", "long"},
                {"Single", "float"},
                {"Double", "double"},
                {"Decimal", "double"},
                {"String", "string"},
                {"DateTime", "date"}
            };

        private static readonly SwaggerGeneratorConfig Config = new SwaggerGeneratorConfig();

        public static SwaggerGenerator GetInstance()
        {
            if (Config.ApiExplorer == null)
                throw new InvalidOperationException("SwaggerGenerator ApiExplorer not set");
            if (Config.BasePathAccessor == null)
                throw new InvalidOperationException("SwaggerGenerator BasePath not set");

            return new SwaggerGenerator(Config);
        }

        public static SwaggerGeneratorConfig Configure()
        {
            return Config;
        }

        private readonly SwaggerGeneratorConfig _config;

        private SwaggerGenerator(SwaggerGeneratorConfig config)
        {
            _config = config;
        }

        public SwaggerSpec GenerateSpec()
        {
            // Group ApiDescriptions by controller name - each group corresponds to an ApiDeclaration
            var descriptionGroups = _config.ApiExplorer.ApiDescriptions
                .Where(ad => !ad.RelativePath.ToLower().StartsWith("swagger"))
                .GroupBy(ad => ad.ActionDescriptor.ControllerDescriptor.ControllerName)
                .ToArray();

            return new SwaggerSpec
                {
                    ResourceListing = GenerateResourceListing(descriptionGroups),
                    ApiDeclarations = GenerateApiDeclarations(descriptionGroups)
                };
        }

        private ResourceListing GenerateResourceListing(IEnumerable<IGrouping<string, ApiDescription>> descriptionGroups)
        {
            return new ResourceListing
                {
                    apiVersion = "1.0",
                    swaggerVersion = SwaggerVersion,
                    basePath = _config.BasePathAccessor(),
                    apis = descriptionGroups.Select(dg => new ResourceLink {path = "/swagger/api-docs/" + dg.Key})
                };
        }

        private Dictionary<string, ApiDeclaration> GenerateApiDeclarations(IEnumerable<IGrouping<string, ApiDescription>> descriptionGroups)
        {
            return descriptionGroups
                .ToDictionary(dg => dg.Key, DescriptionGroupToApiDeclaration);
        }

        private ApiDeclaration DescriptionGroupToApiDeclaration(IGrouping<string, ApiDescription> descriptionGroup)
        {
            // Group further by relative path - each group corresponds to an ApiSpec
            var apiSpecs = descriptionGroup
                .GroupBy(ad => ad.RelativePath)
                .Select(DescriptionGroupToApiSpec);

            return new ApiDeclaration
                {
                    apiVersion = "1.0",
                    swaggerVersion = SwaggerVersion,
                    basePath = _config.BasePathAccessor(),
                    resourcePath = descriptionGroup.Key,
                    apis = apiSpecs,
                    models = BuildModels(descriptionGroup)
                };
        }

        private ApiSpec DescriptionGroupToApiSpec(IGrouping<string, ApiDescription> descriptionGroup)
        {
            return new ApiSpec
                {
                    path = "/" + descriptionGroup.Key,
                    description = String.Empty,
                    operations = descriptionGroup.Select(DescriptionToOperationSpec)
                };
        }

        private ApiOperationSpec DescriptionToOperationSpec(ApiDescription description)
        {
            var queryStart = description.RelativePath.IndexOf('?');
            var queryString = (queryStart == -1) ? String.Empty : description.RelativePath.Substring(queryStart);

            var paramSpecs = description.ParameterDescriptions
                .Select(pd => ParamDescriptionToParameterSpec(pd, queryString.Contains(pd.Name)));

            var operationSpec = new ApiOperationSpec
                {
                    httpMethod = description.HttpMethod.Method,
                    nickname = description.ActionDescriptor.ControllerDescriptor.ControllerName,
                    parameters = paramSpecs,
                    summary = description.Documentation
                };

            foreach (var filter in _config.OperationSpecFilters)
            {
                filter.UpdateSpec(description, operationSpec);
            }

            return operationSpec;
        }

        private ApiParameterSpec ParamDescriptionToParameterSpec(ApiParameterDescription parameterDescription, bool isInQueryString)
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

            return new ApiParameterSpec
                {
                    paramType = paramType,
                    name = parameterDescription.Name,
                    description = parameterDescription.Documentation,
                    dataType = TypeToDataType(parameterDescription.ParameterDescriptor.ParameterType),
                    required = !parameterDescription.ParameterDescriptor.IsOptional
                };
        }

        private string TypeToDataType(Type type)
        {
            return PrimtiveTypeMap[type.Name] ?? type.Name;
        }

        private Dictionary<string, ModelSpec> BuildModels(IEnumerable<ApiDescription> apiDescriptions)
        {
            var apiTypes = apiDescriptions
                .SelectMany(dg => dg.ParameterDescriptions)
                .Select(pd => pd.ParameterDescriptor.ParameterType)
                .Distinct();

            var modelSpecs = new Dictionary<string, ModelSpec>();
            AmmendModelSpecsIfNeccessary(apiTypes, modelSpecs);

            return modelSpecs;
        }

        private void AmmendModelSpecsIfNeccessary(IEnumerable<Type> apiTypes, Dictionary<string, ModelSpec> modelSpecs)
        {
            foreach (var type in apiTypes)
            {        
                if (PrimtiveTypeMap.ContainsKey(type.Name)) continue;

                var dataType = TypeToDataType(type);
                if (modelSpecs.ContainsKey(dataType)) continue;
                            
                var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                var propertySpecs = properties
                    .ToDictionary(pi => pi.Name, PropertyInfoToPropertySpec);
                            
                modelSpecs.Add(type.Name, new ModelSpec { id = type.Name, properties = propertySpecs });
        
                AmmendModelSpecsIfNeccessary(properties.Select(p => p.PropertyType), modelSpecs);
            }
        }

        private ModelPropertySpec PropertyInfoToPropertySpec(PropertyInfo propertyInfo)
        {
            return new ModelPropertySpec
            {
                type = TypeToDataType(propertyInfo.PropertyType),
                required = true // TODO: Discover this
            };
        }
    }
}