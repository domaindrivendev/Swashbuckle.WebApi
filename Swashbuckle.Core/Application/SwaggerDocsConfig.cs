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
        private Func<HttpRequestMessage, string> _hostNameResolver;
        private IEnumerable<string> _schemes;
        private IDictionary<string, SecuritySchemeBuilder> _securitySchemeBuilders;
        private readonly IList<Func<ISchemaFilter>> _schemaFilters;
        private readonly IList<Func<IOperationFilter>> _operationFilters;
        private readonly IList<Func<IDocumentFilter>> _documentFilters;

        private Func<ApiDescription, string, bool> _versionSupportResolver;
        private Func<IEnumerable<ApiDescription>, ApiDescription> _conflictingActionsResolver;

        public SwaggerDocsConfig(Func<HttpRequestMessage, string> hostNameResolver)
        {
            _hostNameResolver = hostNameResolver;
            _securitySchemeBuilders = new Dictionary<string, SecuritySchemeBuilder>();
            _schemaFilters = new List<Func<ISchemaFilter>>();
            _operationFilters = new List<Func<IOperationFilter>>();
            _documentFilters = new List<Func<IDocumentFilter>>();

            VersionInfoBuilder = new VersionInfoBuilder();
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

        internal ISwaggerProvider GetSwaggerProvider(HttpRequestMessage swaggerRequest)
        {
            // If schemes have not been explicitly provided, default to scheme from the swagger request
            var schemes = _schemes ?? new[] { swaggerRequest.RequestUri.Scheme };

            var securityDefintitions = _securitySchemeBuilders.Any()
                ? _securitySchemeBuilders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Build())
                : null;

            var settings = new SwaggerGeneratorSettings(
                versionSupportResolver: _versionSupportResolver, // TODO: handle null value
                apiVersions: VersionInfoBuilder.Build(),
                hostName: _hostNameResolver(swaggerRequest),
                virtualPathRoot: swaggerRequest.GetConfiguration().VirtualPathRoot,
                schemes: schemes, 
                securityDefinitions: securityDefintitions, 
                schemaFilters: _schemaFilters.Select(factory => factory()),
                operationFilters: _operationFilters.Select(factory => factory()),
                documentFilters: _documentFilters.Select(factory => factory()),
                conflictingActionsResolver: _conflictingActionsResolver
            );

            return new SwaggerGenerator(swaggerRequest.GetConfiguration().Services.GetApiExplorer(), settings);
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