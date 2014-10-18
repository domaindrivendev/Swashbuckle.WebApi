using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Swashbuckle.WebAssets
{
    public class EmbeddedWebAssetProvider : IWebAssetProvider
    {
        private IDictionary<string, string> _replacements;
        private IDictionary<string, EmbeddedResourceDescriptor> _customWebAssets;

        public EmbeddedWebAssetProvider(
            IDictionary<string, string> replacements,
            IDictionary<string, EmbeddedResourceDescriptor> customWebAssets)
        {
            _replacements = replacements;
            _customWebAssets = customWebAssets;
        }

        public WebAsset GetWebAssetFor(string path)
        {
            var stream = GetEmbeddedResourceStreamFor(path);
            var mediaType = InferMediaTypeFrom(path);
            return new WebAsset(stream, mediaType);
        }

        private Stream GetEmbeddedResourceStreamFor(string path)
        {
            EmbeddedResourceDescriptor customEmbeddedResource;
            var isCustom = _customWebAssets.TryGetValue(path, out customEmbeddedResource);

            var assembly = isCustom ? customEmbeddedResource.ContainingAssembly : GetType().Assembly;
            var name = isCustom ? customEmbeddedResource.Name : path;

            var stream = assembly.GetManifestResourceStream(name);
            if (stream == null)
                throw new WebAssetNotFound();

            return isCustom ? stream.FindAndReplace(_replacements) : stream;
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