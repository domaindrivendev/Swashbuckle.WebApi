using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.Http.Description;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Swashbuckle.Models
{
    public class ApiExplorerAdapter : ISwaggerSpec
    {
        protected const string SwaggerVersion = "1.2";

        private readonly IEnumerable<IGrouping<string, ApiDescription>> _apiGroups;
        private readonly Func<string> _basePathAccessor;
        private readonly IEnumerable<IOperationSpecFilter> _postFilters;
        private readonly Lazy<ResourceListing> _resourceListing;
        private readonly Lazy<Dictionary<string, ApiDeclaration>> _apiDeclarations;

        public ApiExplorerAdapter(
            IApiExplorer apiExplorer,
            IApiGroupingStrategy apiGroupingStrategy,
            IEnumerable<IOperationSpecFilter> postFilters,
            Func<string> basePathAccessor)
        {
            // Initial grouping - Api Declaration for each group
            _apiGroups = apiGroupingStrategy.Group(apiExplorer.ApiDescriptions);

            _postFilters = postFilters;
            _basePathAccessor = basePathAccessor;
            _resourceListing = new Lazy<ResourceListing>(GenerateResourceListing);
            _apiDeclarations = new Lazy<Dictionary<string, ApiDeclaration>>(GenerateApiDeclarations);
        }

        public ResourceListing GetResourceListing()
        {
            return _resourceListing.Value;
        }

        public ApiDeclaration GetApiDeclaration(string resourcePath)
        {
            return _apiDeclarations.Value[resourcePath];
        }

        private ResourceListing GenerateResourceListing()
        {
            return new ResourceListing()
                {
                    apiVersion = "1.0",
                    swaggerVersion = SwaggerVersion,
                    apis = _apiGroups.Select(group => new ApiDeclarationLink { path = group.Key }).ToArray()
                };
        }

        private Dictionary<string, ApiDeclaration> GenerateApiDeclarations()
        {
            return _apiGroups
                .ToDictionary(group => group.Key, DescriptionGroupToApiDeclaration);
        }

        private ApiDeclaration DescriptionGroupToApiDeclaration(IGrouping<string, ApiDescription> descriptionGroup)
        {
            // Create JsonSchemas for all types in the ApiDescription
            var apiTypes = descriptionGroup
                .SelectMany(ad => ad
                    .ParameterDescriptions.Select(pd => pd.ParameterDescriptor.ParameterType)
                    .Union(new[] {ad.ActionDescriptor.ReturnType}))
                .Where(t => t != null)
                .Distinct();

            var schemaGenerator = new JsonSchemaGenerator();
            var schemas = apiTypes.Select(schemaGenerator.Generate);

//            // Group further by relative path - ApiSpec for each group
//            var apiSpecs = descriptionGroup
//                .GroupBy(ad => ad.RelativePath)
//                .Select(group => DescriptionGroupToApiSpec(group, schemas))
//                .ToList();

            return new ApiDeclaration
                {
                    apiVersion = "1.0",
                    swaggerVersion = SwaggerVersion,
                    basePath = _basePathAccessor(),
                    resourcePath = descriptionGroup.Key,
                    //apis = apiSpecs,
                    //models = modelSpecsBuilder.Build()
                };
        }

        private JObject DescriptionGroupToApiSpec(IGrouping<string, ApiDescription> descriptionGroup, IEnumerable<JsonSchema> jsonSchemas)
        {
//            var pathParts = descriptionGroup.Key.Split('?');
//            var pathOnly = pathParts[0];
//            var queryString = pathParts.Length == 1 ? String.Empty : pathParts[1];
            
//            var operationSpecs = descriptionGroup
//                .Select(group => DescriptionToOperationSpec(group, jsonSchemas))
//                .ToList();

            return JObject.FromObject(new
                {
                    path = descriptionGroup.Key.Split('?').First(),
                    //operations = operationSpecs
                });
        }

        private JObject DescriptionToOperationSpec(ApiDescription description, IEnumerable<JsonSchema> jsonSchemas)
        {
            throw new NotImplementedException();
        }
    }
}
