using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;

namespace Swashbuckle.Swagger
{
    public class SwaggerGeneratorOptions
    {
        public SwaggerGeneratorOptions(
            Func<ApiDescription, string, bool> versionSupportResolver = null,
            IEnumerable<string> schemes = null,
            IDictionary<string, SecurityScheme> securityDefinitions = null,
            bool ignoreObsoleteActions = false,
            Func<ApiDescription, string> groupingKeySelector = null,
            IComparer<string> groupingKeyComparer = null,
            IDictionary<Type, Func<Schema>> customSchemaMappings = null,
            IEnumerable<ISchemaFilter> schemaFilters = null,
            IEnumerable<IModelFilter> modelFilters = null,
            bool ignoreObsoleteProperties = false,
            Func<Type, string> schemaIdSelector = null, 
            bool describeAllEnumsAsStrings = false,
            bool describeStringEnumsInCamelCase = false,
            IEnumerable<IOperationFilter> operationFilters = null,
            IEnumerable<IDocumentFilter> documentFilters = null,
            Func<IEnumerable<ApiDescription>, ApiDescription> conflictingActionsResolver = null,
            IApiDescriptionFilter apiDescriptionFilter = null
            )
        {
            VersionSupportResolver = versionSupportResolver;
            Schemes = schemes;
            SecurityDefinitions = securityDefinitions;
            IgnoreObsoleteActions = ignoreObsoleteActions;
            GroupingKeySelector = groupingKeySelector ?? DefaultGroupingKeySelector;
            GroupingKeyComparer = groupingKeyComparer ?? Comparer<string>.Default;
            CustomSchemaMappings = customSchemaMappings ?? new Dictionary<Type, Func<Schema>>();
            SchemaFilters = schemaFilters ?? new List<ISchemaFilter>();
            ModelFilters = modelFilters ?? new List<IModelFilter>();
            IgnoreObsoleteProperties = ignoreObsoleteProperties;
            SchemaIdSelector = schemaIdSelector ?? DefaultSchemaIdSelector;
            DescribeAllEnumsAsStrings = describeAllEnumsAsStrings;
            DescribeStringEnumsInCamelCase = describeStringEnumsInCamelCase;
            OperationFilters = operationFilters ?? new List<IOperationFilter>();
            DocumentFilters = documentFilters ?? new List<IDocumentFilter>();
            ConflictingActionsResolver = conflictingActionsResolver ?? DefaultConflictingActionsResolver;
            ApiDescriptionFilter = apiDescriptionFilter;
        }

        public Func<ApiDescription, string, bool> VersionSupportResolver { get; private set; }

        public IEnumerable<string> Schemes { get; private set; }

        public IDictionary<string, SecurityScheme> SecurityDefinitions { get; private set; }

        public bool IgnoreObsoleteActions { get; private set; }

        public Func<ApiDescription, string> GroupingKeySelector { get; private set; }

        public IComparer<string> GroupingKeyComparer { get; private set; }

        public IDictionary<Type, Func<Schema>> CustomSchemaMappings { get; private set; }

        public IEnumerable<ISchemaFilter> SchemaFilters { get; private set; }

        public IEnumerable<IModelFilter> ModelFilters { get; private set; }

        public bool IgnoreObsoleteProperties { get; private set; }

        public Func<Type, string> SchemaIdSelector { get; private set; }

        public bool DescribeAllEnumsAsStrings { get; private set; }

        public bool DescribeStringEnumsInCamelCase { get; private set; }

        public IEnumerable<IOperationFilter> OperationFilters { get; private set; }

        public IEnumerable<IDocumentFilter> DocumentFilters { get; private set; }

        public Func<IEnumerable<ApiDescription>, ApiDescription> ConflictingActionsResolver { get; private set; }

        public IApiDescriptionFilter ApiDescriptionFilter { get; private set; }

        private string DefaultGroupingKeySelector(ApiDescription apiDescription)
        {
            return apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName;
        }

        private static string DefaultSchemaIdSelector(Type type)
        {
            return type.FriendlyId();
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