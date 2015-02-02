using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json.Converters;
using Swashbuckle.Swagger;
using Swashbuckle.Swagger.Filters;

namespace Swashbuckle.Application
{
    public class SwaggerDocsConfig
    {
        private VersionInfoBuilder _versionInfoBuilder;
        private Func<ApiDescription, string, bool> _versionSupportResolver;
        private IEnumerable<string> _schemes;
        private IDictionary<string, SecuritySchemeBuilder> _securitySchemeBuilders;
        private bool _ignoreObsoleteActions;
        private Func<ApiDescription, string> _groupingKeySelector;
        private IComparer<string> _groupingKeyComparer;
        private readonly IDictionary<Type, Func<Schema>> _customSchemaMappings;
        private readonly IList<Func<ISchemaFilter>> _schemaFilters;
        private bool _ignoreObsoleteProperties;
        private bool _useFullTypeNameInSchemaIds;
        private bool _describeAllEnumsAsStrings;
        private readonly IList<Func<IOperationFilter>> _operationFilters;
        private readonly IList<Func<IDocumentFilter>> _documentFilters;
        private Func<IEnumerable<ApiDescription>, ApiDescription> _conflictingActionsResolver;
        private Func<HttpRequestMessage, string> _rootUrlResolver;

        public SwaggerDocsConfig()
        {
            _versionInfoBuilder = new VersionInfoBuilder();
            _securitySchemeBuilders = new Dictionary<string, SecuritySchemeBuilder>();
            _ignoreObsoleteActions = false;
            _customSchemaMappings = new Dictionary<Type, Func<Schema>>();
            _schemaFilters = new List<Func<ISchemaFilter>>();
            _ignoreObsoleteProperties = false;
            _useFullTypeNameInSchemaIds = false;
            _describeAllEnumsAsStrings = false;
            _operationFilters = new List<Func<IOperationFilter>>();
            _documentFilters = new List<Func<IDocumentFilter>>();
            _rootUrlResolver = DefaultRootUrlResolver;

            OperationFilter<HandleFromUriParams>();
        }

        public InfoBuilder SingleApiVersion(string version, string title)
        {
            _versionSupportResolver = null;
            _versionInfoBuilder = new VersionInfoBuilder();
            return _versionInfoBuilder.Version(version, title);
        }

        public void MultipleApiVersions(
            Func<ApiDescription, string, bool> versionSupportResolver,
            Action<VersionInfoBuilder> configure)
        {
            _versionSupportResolver = versionSupportResolver;
            _versionInfoBuilder = new VersionInfoBuilder();
            configure(_versionInfoBuilder);
        }

        public void Schemes(IEnumerable<string> schemes)
        {
            _schemes = schemes;
        }

        public BasicAuthSchemeBuilder BasicAuth(string name)
        {
            var schemeBuilder = new BasicAuthSchemeBuilder();
            _securitySchemeBuilders[name] = schemeBuilder;
            return schemeBuilder;
        }

        public ApiKeySchemeBuilder ApiKey(string name)
        {
            var schemeBuilder = new ApiKeySchemeBuilder();
            _securitySchemeBuilders[name] = schemeBuilder;
            return schemeBuilder;
        }

        public OAuth2SchemeBuilder OAuth2(string name)
        {
            var schemeBuilder = new OAuth2SchemeBuilder();
            _securitySchemeBuilders[name] = schemeBuilder;
            return schemeBuilder;
        }

        public void IgnoreObsoleteActions()
        {
            _ignoreObsoleteActions = true;
        }

        public void GroupActionsBy(Func<ApiDescription, string> keySelector)
        {
            _groupingKeySelector = keySelector;
        }

        public void OrderActionGroupsBy(IComparer<string> keyComparer)
        {
            _groupingKeyComparer = keyComparer;
        }

        public void MapType<T>(Func<Schema> factory)
        {
            _customSchemaMappings.Add(typeof(T), factory);
        }

        public void SchemaFilter<TFilter>()
            where TFilter : ISchemaFilter, new()
        {
            SchemaFilter(() => new TFilter());
        }

        public void SchemaFilter(Func<ISchemaFilter> factory)
        {
            _schemaFilters.Add(factory);
        }

        public void UseFullTypeNameInSchemaIds()
        {
            _useFullTypeNameInSchemaIds = true;
        }

        public void DescribeAllEnumsAsStrings()
        {
            _describeAllEnumsAsStrings = true;
        }

        public void IgnoreObsoleteProperties()
        {
            _ignoreObsoleteProperties = true;
        }

        public void OperationFilter<TFilter>()
            where TFilter : IOperationFilter, new()
        {
            OperationFilter(() => new TFilter());
        }

        public void OperationFilter(Func<IOperationFilter> factory)
        {
            _operationFilters.Add(factory);
        }

        public void DocumentFilter<TFilter>()
            where TFilter : IDocumentFilter, new()
        {
            DocumentFilter(() => new TFilter());
        }

        public void DocumentFilter(Func<IDocumentFilter> factory)
        {
            _documentFilters.Add(factory);
        }

        public void IncludeXmlComments(string filePath)
        {
            OperationFilter(() => new ApplyXmlActionComments(filePath));
            SchemaFilter(() => new ApplyXmlTypeComments(filePath));
        }

        public void ResolveConflictingActions(Func<IEnumerable<ApiDescription>, ApiDescription> conflictingActionsResolver)
        {
            _conflictingActionsResolver = conflictingActionsResolver;
        }

        public void RootUrl(Func<HttpRequestMessage, string> rootUrlResolver)
        {
            _rootUrlResolver = rootUrlResolver;
        }

        internal ISwaggerProvider GetSwaggerProvider(HttpRequestMessage swaggerRequest)
        {
            var httpConfig = swaggerRequest.GetConfiguration();

            var securityDefintitions = _securitySchemeBuilders.Any()
                ? _securitySchemeBuilders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Build())
                : null;

            var describeAllEnumsAsStrings = _describeAllEnumsAsStrings
                || httpConfig.HasJsonConverterOfType<StringEnumConverter>();

            var options = new SwaggerGeneratorOptions(
                versionSupportResolver: _versionSupportResolver,
                schemes: _schemes,
                securityDefinitions: securityDefintitions,
                ignoreObsoleteActions: _ignoreObsoleteActions,
                groupingKeySelector: _groupingKeySelector,
                groupingKeyComparer: _groupingKeyComparer,
                customSchemaMappings: _customSchemaMappings,
                schemaFilters: _schemaFilters.Select(factory => factory()),
                ignoreObsoleteProperties: _ignoreObsoleteProperties,
                useFullTypeNameInSchemaIds: _useFullTypeNameInSchemaIds,
                describeAllEnumsAsStrings: describeAllEnumsAsStrings,
                operationFilters: _operationFilters.Select(factory => factory()),
                documentFilters: _documentFilters.Select(factory => factory()),
                conflictingActionsResolver: _conflictingActionsResolver
            );

            return new SwaggerGenerator(
                httpConfig.Services.GetApiExplorer(),
                httpConfig.GetJsonContractResolver(),
                _versionInfoBuilder.Build(),
                options);
        }

        internal string GetRootUrl(HttpRequestMessage swaggerRequest)
        {
            return _rootUrlResolver(swaggerRequest);
        }

        internal IEnumerable<string> GetApiVersions()
        {
            return _versionInfoBuilder.Build().Select(entry => entry.Key);
        }

        public static string DefaultRootUrlResolver(HttpRequestMessage request)
        {
            var scheme = GetHeaderValue(request, "X-Forwarded-Proto") ?? request.RequestUri.Scheme;
            var host = GetHeaderValue(request, "X-Forwarded-Host") ?? request.RequestUri.Host;
            var port = GetHeaderValue(request, "X-Forwarded-Port") ?? request.RequestUri.Port.ToString(CultureInfo.InvariantCulture);

            var httpConfiguration = request.GetConfiguration();
            var virtualPathRoot = httpConfiguration != null ? httpConfiguration.VirtualPathRoot.TrimEnd('/') : string.Empty;
            
            return string.Format("{0}://{1}:{2}{3}", scheme, host, port, virtualPathRoot);
        }

        private static string GetHeaderValue(HttpRequestMessage request, string headerName)
        {
            IEnumerable<string> list;
            return request.Headers.TryGetValues(headerName, out list) ? list.FirstOrDefault() : null;
        }
    }
}