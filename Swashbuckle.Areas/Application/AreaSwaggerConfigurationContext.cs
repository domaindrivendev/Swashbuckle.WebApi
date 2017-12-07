using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Swashbuckle.Application
{
    public class AreaSwaggerConfigurationContext
    {
        public Func<HttpRequestMessage, string> RootUrlResolver { get; }
        public IEnumerable<string> DiscoveryPaths { get; }
        public string RouteTemplate { get; }
        public SwaggerDocsConfig Config { get; }


        public AreaSwaggerConfigurationContext(SwaggerDocsConfig config, IEnumerable<string> discoveryPaths, string routeTemplate)
        {
            Config = config;
            RootUrlResolver = config.GetRootUrl;
            DiscoveryPaths = discoveryPaths;
            RouteTemplate = routeTemplate;
        }

    }
}