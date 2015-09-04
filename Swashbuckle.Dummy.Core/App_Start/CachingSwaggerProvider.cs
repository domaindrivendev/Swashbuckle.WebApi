using Swashbuckle.Swagger;
using System.Collections.Concurrent;

namespace Swashbuckle.Dummy.App_Start
{
    public class CachingSwaggerProvider : ISwaggerProvider
    {
        private static ConcurrentDictionary<string, SwaggerDocument> _cache =
            new ConcurrentDictionary<string, SwaggerDocument>();

        private readonly ISwaggerProvider _swaggerProvider;

        public CachingSwaggerProvider(ISwaggerProvider swaggerProvider)
        {
            _swaggerProvider = swaggerProvider;
        }

        public SwaggerDocument GetSwagger(string rootUrl, string apiVersion)
        {
            var cacheKey = string.Format("{0}_{1}", rootUrl, apiVersion);
            return _cache.GetOrAdd(cacheKey, (key) => _swaggerProvider.GetSwagger(rootUrl, apiVersion));
        }
    }
}
