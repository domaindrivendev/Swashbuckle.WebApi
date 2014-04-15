using System;
using System.Collections.Concurrent;
using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    internal class CachingSwaggerProvider : ISwaggerProvider
    {
        private static readonly ConcurrentDictionary<string, ResourceListing> ListingCache = new ConcurrentDictionary<string, ResourceListing>();
        private static readonly ConcurrentDictionary<string, ApiDeclaration> DeclarationCache = new ConcurrentDictionary<string, ApiDeclaration>();

        private readonly ISwaggerProvider _swaggerProvider;

        public CachingSwaggerProvider(ISwaggerProvider swaggerProvider)
        {
            _swaggerProvider = swaggerProvider;
        }

        public ResourceListing GetListing(string basePath, string version)
        {
            var key = String.Format("{0}_{1}", basePath, version);
            return ListingCache.GetOrAdd(key, (k) => _swaggerProvider.GetListing(basePath, version));
        }

        public ApiDeclaration GetDeclaration(string basePath, string version, string resourceName)
        {
            var key = String.Format("{0}_{1}_{2}", basePath, version, resourceName);
            return DeclarationCache.GetOrAdd(key, (k) => _swaggerProvider.GetDeclaration(basePath, version, resourceName));
        }
    }
}