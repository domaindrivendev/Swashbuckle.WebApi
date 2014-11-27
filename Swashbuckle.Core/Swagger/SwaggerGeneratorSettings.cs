using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;

namespace Swashbuckle.Swagger
{
    public class SwaggerGeneratorSettings
    {
        public SwaggerGeneratorSettings(
            Func<ApiDescription, string, bool> versionSupportResolver,
            IDictionary<string, Info> apiVersions,
            IEnumerable<string> schemes,
            Func<ApiDescription, string> groupingKeySelector,
            IComparer<string> groupingKeyComparer,
            IDictionary<string, SecurityScheme> securityDefinitions = null,
            IDictionary<Type, Func<Schema>> customSchemaMappings = null,
            IEnumerable<ISchemaFilter> schemaFilters = null,
            IEnumerable<IOperationFilter> operationFilters = null,
            IEnumerable<IDocumentFilter> documentFilters = null,
            Func<IEnumerable<ApiDescription>, ApiDescription> conflictingActionsResolver = null)
        {
            VersionSupportResolver = versionSupportResolver;
            ApiVersions = apiVersions;
            Schemes = schemes;
            GroupingKeySelector = groupingKeySelector ?? DefaultGroupingKeySelector;
            GroupingKeyComparer = groupingKeyComparer ?? Comparer<string>.Default;
            SecurityDefinitions = securityDefinitions;
            CustomSchemaMappings = customSchemaMappings ?? new Dictionary<Type, Func<Schema>>();
            SchemaFilters = schemaFilters ?? new List<ISchemaFilter>();
            OperationFilters = operationFilters ?? new List<IOperationFilter>();
            DocumentFilters = documentFilters ?? new List<IDocumentFilter>();
            ConflictingActionsResolver = conflictingActionsResolver ?? DefaultConflictingActionsResolver;
        }

        public Func<ApiDescription, string, bool> VersionSupportResolver { get; private set; }

        public IDictionary<string, Info> ApiVersions { get; private set; }

        public IEnumerable<string> Schemes { get; private set; }

        public Func<ApiDescription, string> GroupingKeySelector { get; private set; }

        public IComparer<string> GroupingKeyComparer { get; private set; }

        public IDictionary<string, SecurityScheme> SecurityDefinitions { get; private set; }

        public IDictionary<Type, Func<Schema>> CustomSchemaMappings { get; private set; }

        public IEnumerable<ISchemaFilter> SchemaFilters { get; private set; }

        public IEnumerable<IOperationFilter> OperationFilters { get; private set; }

        public IEnumerable<IDocumentFilter> DocumentFilters { get; private set; }

        public Func<IEnumerable<ApiDescription>, ApiDescription> ConflictingActionsResolver { get; private set; }

        private string DefaultGroupingKeySelector(ApiDescription apiDescription)
        {
            return apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName;
        }

        private ApiDescription DefaultConflictingActionsResolver(IEnumerable<ApiDescription> apiDescriptions)
        {
            var first = apiDescriptions.First();
            throw new NotSupportedException(String.Format(
                "Not supported by Swagger 2.0: Multiple operations with path '{0}' and method '{1}'. " +
                "See the config setting - \"ResolveConflictingActions\" for a potential workaround",
                first.RelativePathSansQueryString(), first.HttpMethod));
        }
    }
}