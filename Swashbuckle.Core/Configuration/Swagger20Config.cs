using System;
using System.Net.Http;
using System.Web.Http;
using System.Linq;
using System.Collections.Generic;
using Swashbuckle.Swagger20;

namespace Swashbuckle.Configuration
{
    public class Swagger20Config
    {
        private InfoBuilder _infoBuilder;
        private IEnumerable<string> _schemes;

        private readonly IList<Func<IOperationFilter>> _operationFilters;
        private readonly IList<Func<IDocumentFilter>> _documentFilters;

        public Swagger20Config()
        {
            _infoBuilder = new InfoBuilder();
            _schemes = new[] { "http" };
            _operationFilters = new List<Func<IOperationFilter>>();
            _documentFilters = new List<Func<IDocumentFilter>>();

            HostNameResolver = (req) => req.RequestUri.Host;
        }

        internal Func<HttpRequestMessage, string> HostNameResolver { get; private set; }

        public InfoBuilder ApiVersion(string apiVersion)
        {
            return _infoBuilder.Version(apiVersion);
        }

        public void HostName(Func<HttpRequestMessage, string> hostNameResolver)
        {
            HostNameResolver = hostNameResolver;
        }

        public void Schemes(IEnumerable<string> schemes)
        {
            _schemes = schemes;
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
                info: _infoBuilder.Build(),
                host: HostNameResolver(request),
                virtualPathRoot: httpConfig.VirtualPathRoot,
                schemes: _schemes,
                operationFilters: _operationFilters.Select(factory => factory()),
                documentFilters: _documentFilters.Select(factory => factory()));

            return new SwaggerGenerator(apiExplorer, settings);
        }
    }
}
