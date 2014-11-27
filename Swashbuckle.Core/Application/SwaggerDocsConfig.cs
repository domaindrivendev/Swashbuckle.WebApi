using System;
using System.Net.Http;
using System.Web.Http;
using System.Linq;
using System.Collections.Generic;
using System.Web.Http.Description;
using Swashbuckle.Swagger;
using Swashbuckle.SwaggerExtensions;

namespace Swashbuckle.Application
{
    public class SwaggerDocsConfig
    {
        private IEnumerable<string> _schemes;
        private IDictionary<string, SecuritySchemeBuilder> _securitySchemeBuilders;
        private readonly IDictionary<Type, Func<Schema>> _customSchemaMappings;
        private readonly IList<Func<ISchemaFilter>> _schemaFilters;
        private readonly IList<Func<IOperationFilter>> _operationFilters;
        private readonly IList<Func<IDocumentFilter>> _documentFilters;

        private Func<ApiDescription, string, bool> _versionSupportResolver;
        private Func<IEnumerable<ApiDescription>, ApiDescription> _conflictingActionsResolver;
        private Func<ApiDescription, string> _groupingKeySelector;
        private IComparer<string> _groupingKeyComparer;

        public SwaggerDocsConfig()
        {
            _securitySchemeBuilders = new Dictionary<string, SecuritySchemeBuilder>();
            _customSchemaMappings = new Dictionary<Type, Func<Schema>>();
            _schemaFilters = new List<Func<ISchemaFilter>>();
            _operationFilters = new List<Func<IOperationFilter>>();
            _documentFilters = new List<Func<IDocumentFilter>>();

            VersionInfoBuilder = new VersionInfoBuilder();

            OperationFilter<HandleParamsFromUri>();
        }

        internal VersionInfoBuilder VersionInfoBuilder { get; private set; }

        public InfoBuilder SingleApiVersion(string version, string title)
        {
            _versionSupportResolver = (apiDesc, requestedApiVersion) => requestedApiVersion == version;
            VersionInfoBuilder = new VersionInfoBuilder();
            return VersionInfoBuilder.Version(version, title);
        }

        public void MultipleApiVersions(
            Func<ApiDescription, string, bool> versionSupportResolver,
            Action<VersionInfoBuilder> configureVersionInfos)
        {
            _versionSupportResolver = versionSupportResolver;
            VersionInfoBuilder = new VersionInfoBuilder();
            configureVersionInfos(VersionInfoBuilder);
        }

        public void Schemes(IEnumerable<string> schemes)
        {
            _schemes = schemes;
        }

        public void GroupActionsBy(Func<ApiDescription, string> keySelector)
        {
            _groupingKeySelector = keySelector;
        }

        public void OrderActionGroupsBy(IComparer<string> keyComparer)
        {
            _groupingKeyComparer = keyComparer;
        }

        public BasicAuthSchemeBuilder BasicAuth(string name)
        {
            return AddSecuritySchemeBuilder<BasicAuthSchemeBuilder>(name);
        }

        public ApiKeySchemeBuilder ApiKey(string name)
        {
            return AddSecuritySchemeBuilder<ApiKeySchemeBuilder>(name);
        }

        public OAuth2SchemeBuilder OAuth2(string name)
        {
            return AddSecuritySchemeBuilder<OAuth2SchemeBuilder>(name);
        }

        public void MapType<T>(Func<Schema> factory)
        {
            _customSchemaMappings.Add(typeof(T), factory);
        }

        public void SchemaFilter<TFilter>()
            where TFilter : ISchemaFilter, new()
        {
            _schemaFilters.Add(() => new TFilter());
        }

        public void OperationFilter<TFilter>()
            where TFilter : IOperationFilter, new()
        {
            _operationFilters.Add(() => new TFilter());
        }

        public void DocumentFilter<TFilter>()
            where TFilter : IDocumentFilter, new()
        {
            _documentFilters.Add(() => new TFilter());
        }

        public void IncludeXmlComments(string filePath)
        {
            _operationFilters.Add(() => new ApplyXmlActionComments(filePath));
            _schemaFilters.Add(() => new ApplyXmlTypeComments(filePath));
        }

        public void ResolveConflictingActions(Func<IEnumerable<ApiDescription>, ApiDescription> conflictingActionsResolver)
        {
            _conflictingActionsResolver = conflictingActionsResolver;
        }

        public SwaggerGeneratorSettings ToGeneratorSettings()
        {
            var securityDefintitions = _securitySchemeBuilders.Any()
                ? _securitySchemeBuilders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Build())
                : null;

            return new SwaggerGeneratorSettings(
                versionSupportResolver: _versionSupportResolver, // TODO: handle null value
                apiVersions: VersionInfoBuilder.Build(),
                schemes: _schemes, 
                groupingKeySelector: _groupingKeySelector,
                groupingKeyComparer: _groupingKeyComparer,
                securityDefinitions: securityDefintitions, 
                customSchemaMappings: _customSchemaMappings,
                schemaFilters: _schemaFilters.Select(factory => factory()),
                operationFilters: _operationFilters.Select(factory => factory()),
                documentFilters: _documentFilters.Select(factory => factory()),
                conflictingActionsResolver: _conflictingActionsResolver
            );
        }

        private T AddSecuritySchemeBuilder<T>(string name)
            where T : SecuritySchemeBuilder, new()
        {
            var builder = new T();
            _securitySchemeBuilders[name] = builder;
            return builder;
        }
    }
}