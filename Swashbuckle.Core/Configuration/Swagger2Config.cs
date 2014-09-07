using System;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger2;

namespace Swashbuckle.Configuration
{
    public class Swagger2Config
    {
        private InfoBuilder _infoBuilder;
        private bool _ignoreObsoleteActions;

        public Swagger2Config()
        {
            HostNameResolver = (req) => req.RequestUri.GetLeftPart(UriPartial.Authority);
            _infoBuilder = new InfoBuilder();
        }

        internal Func<HttpRequestMessage, string> HostNameResolver { get; private set; }

        public InfoBuilder ApiVersion(string apiVersion)
        {
            return _infoBuilder.Version(apiVersion);
        }

        public void IgnoreObsoleteActions()
        {
            _ignoreObsoleteActions = true;
        }

        public ISwaggerProvider GetSwaggerProvider(HttpRequestMessage request)
        {
            var httpConfig = request.GetConfiguration();

            var apiExplorer = httpConfig.Services.GetApiExplorer();

            var settings = new SwaggerGeneratorSettings(
                host: HostNameResolver(request),
                virtualPathRoot: httpConfig.VirtualPathRoot,
                info: _infoBuilder.Build(),
                ignoreObsoleteActions: _ignoreObsoleteActions);

            return new SwaggerGenerator(apiExplorer, settings);
        }
    }
}
