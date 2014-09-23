using System;
using System.Net.Http;
using System.Web.Http;
using System.Linq;
using System.Collections.Generic;
using Swashbuckle.Swagger20;
using System.Web.Http.Description;

namespace Swashbuckle.Configuration
{
    public class Swagger20Config
    {
        private Func<ApiDescription, string, bool> _versionSupportResolver;

        private ApiVersionsBuilder _apiVersionsBuilder;
        private IEnumerable<string> _schemes;

        private readonly IList<Func<ISchemaFilter>> _schemaFilters;
        private readonly IList<Func<IOperationFilter>> _operationFilters;
        private readonly IList<Func<IDocumentFilter>> _documentFilters;

        public Swagger20Config()
        {
            HostNameResolver = (req) => req.RequestUri.Host;

            _schemes = new[] { "http" };

            _schemaFilters = new List<Func<ISchemaFilter>>();
            _operationFilters = new List<Func<IOperationFilter>>();
            _documentFilters = new List<Func<IDocumentFilter>>();
        }

        internal Func<HttpRequestMessage, string> HostNameResolver { get; private set; }

        public InfoBuilder SingleApiVersion(string version, string title)
        {
            _versionSupportResolver = (apiDesc, requestedApiVersion) => requestedApiVersion == version;
            _apiVersionsBuilder = new ApiVersionsBuilder();
            return _apiVersionsBuilder.Version(version, title);
        }

        public void MultipleApiVersions(
            Func<ApiDescription, string, bool> versionSupportResolver,
            Action<ApiVersionsBuilder> configureApiVersions)
        {
            _versionSupportResolver = versionSupportResolver;
            configureApiVersions(_apiVersionsBuilder);
        }

        public void HostName(Func<HttpRequestMessage, string> hostNameResolver)
        {
            HostNameResolver = hostNameResolver;
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

        public ISwaggerProvider GetSwaggerProvider(HttpRequestMessage request)
        {
            var httpConfig = request.GetConfiguration();

            var apiExplorer = httpConfig.Services.GetApiExplorer();

            var settings = new SwaggerGeneratorSettings(
                versionSupportResolver: _versionSupportResolver, // TODO: handle null value
                apiVersions: _apiVersionsBuilder.Build(),
                host: HostNameResolver(request),
                virtualPathRoot: httpConfig.VirtualPathRoot,
                schemes: _schemes,
                schemaFilters: _schemaFilters.Select(factory => factory()),
                operationFilters: _operationFilters.Select(factory => factory()),
                documentFilters: _documentFilters.Select(factory => factory()));

            return new SwaggerGenerator(apiExplorer, settings);
        }
    }
}
