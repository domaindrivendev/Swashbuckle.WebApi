using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Swashbuckle.SwaggerUi
{
    public class EmbeddedAssetProvider : IAssetProvider
    {
        private readonly IDictionary<string, EmbeddedAssetDescriptor> _customAssets;
        private readonly IDictionary<string, string> _replacements;

        public EmbeddedAssetProvider(
            IDictionary<string, EmbeddedAssetDescriptor> customAssets,
            IDictionary<string, string> replacements)
        {
            _customAssets = customAssets;
            _replacements = replacements;
        }

        public Asset GetAsset(string rootUrl, string path)
        {
            var stream = GetEmbeddedResourceStreamFor(rootUrl, path);
            var mediaType = InferMediaTypeFrom(path);
            return new Asset(stream, mediaType);
        }

        private Stream GetEmbeddedResourceStreamFor(string rootUrl, string assetPath)
        {
            EmbeddedAssetDescriptor customEmbeddedResource;
            var isCustom = _customAssets.TryGetValue(assetPath, out customEmbeddedResource);

            var assembly = isCustom ? customEmbeddedResource.ContainingAssembly : GetType().Assembly;
            var name = isCustom ? customEmbeddedResource.Name : assetPath;

            var stream = assembly.GetManifestResourceStream(name);
            if (stream == null)
                throw new AssetNotFound();

            var replacements = _replacements
                .Union(new[] { new KeyValuePair<string, string>("%(RootUrl)", rootUrl) })
                .ToDictionary(entry => entry.Key, entry => entry.Value);

            return isCustom ? stream.FindAndReplace(replacements) : stream;
        }

        private static string InferMediaTypeFrom(string path)
        {
            var extension = path.Split('.').Last();

            switch (extension)
            {
                case "css":
                    return "text/css";
                case "js":
                    return "text/javascript";
                case "gif":
                    return "image/gif";
                case "png":
                    return "image/png";
                default:
                    return "text/html";
            }
        }
    }
}