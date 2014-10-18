using System;
using System.Net.Http;
using System.Web.Http;
using System.Linq;
using System.Collections.Generic;
using System.Web.Http.Description;
using Swashbuckle.Swagger20;
using Swashbuckle.SwaggerFilters;

namespace Swashbuckle.Application
{
    public class SwaggerDocsConfig
    {
        private Func<ApiDescription, string, bool> _versionSupportResolver;

        private VersionInfoBuilder _versionInfoBuilder;
        private Func<HttpRequestMessage, string> _hostNameResolver;
        private IEnumerable<string> _schemes;
        private readonly IList<Func<ISchemaFilter>> _schemaFilters;
        private readonly IList<Func<IOperationFilter>> _operationFilters;
        private readonly IList<Func<IDocumentFilter>> _documentFilters;

        public SwaggerDocsConfig()
        {
            _versionInfoBuilder = new VersionInfoBuilder();
            _hostNameResolver = (req) => req.RequestUri.Host;
            _schemaFilters = new List<Func<ISchemaFilter>>();
            _operationFilters = new List<Func<IOperationFilter>>();
            _documentFilters = new List<Func<IDocumentFilter>>();
        }

        public InfoBuilder SingleApiVersion(string version, string title)
        {
            _versionSupportResolver = (apiDesc, requestedApiVersion) => requestedApiVersion == version;
            _versionInfoBuilder = new VersionInfoBuilder();
            return _versionInfoBuilder.Version(version, title);
        }

        public void MultipleApiVersions(
            Func<ApiDescription, string, bool> versionSupportResolver,
            Action<VersionInfoBuilder> configureVersionInfos)
        {
            _versionSupportResolver = versionSupportResolver;
            _versionInfoBuilder = new VersionInfoBuilder();
            configureVersionInfos(_versionInfoBuilder);
        }

        public void HostName(Func<HttpRequestMessage, string> hostNameResolver)
        {
            _hostNameResolver = hostNameResolver;
        }

        public void Schemes(IEnumerable<string> schemes)
        {
            _schemes = schemes;
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

        internal ISwaggerProvider GetSwaggerProvider(HttpRequestMessage request)
        {
            var httpConfig = request.GetConfiguration();

            var apiExplorer = httpConfig.Services.GetApiExplorer();

            // If not explicitly configured, default to the scheme currently being used to access swagger enpoints
            var schemes = _schemes ?? new[] { request.RequestUri.Scheme.ToLower() };

            var settings = new SwaggerGeneratorSettings(
                versionSupportResolver: _versionSupportResolver, // TODO: handle null value
                apiVersions: _versionInfoBuilder.Build(),
                host: _hostNameResolver(request),
                virtualPathRoot: httpConfig.VirtualPathRoot,
                schemes: schemes,
                schemaFilters: _schemaFilters.Select(factory => factory()),
                operationFilters: _operationFilters.Select(factory => factory()),
                documentFilters: _documentFilters.Select(factory => factory()));

            return new SwaggerGenerator(apiExplorer, settings);
        }
    }
}