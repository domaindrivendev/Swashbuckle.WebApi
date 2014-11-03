using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Swashbuckle.WebAssets
{
    public class EmbeddedWebAssetProvider : IWebAssetProvider
    {
        private readonly EmbeddedWebAssetProviderSettings _settings;

        public EmbeddedWebAssetProvider(
            EmbeddedWebAssetProviderSettings settings)
        {
            _settings = settings;
        }

        public WebAsset GetWebAssetFor(string path, string rootUrl)
        {
            var stream = GetEmbeddedResourceStreamFor(path, rootUrl);
            var mediaType = InferMediaTypeFrom(path);
            return new WebAsset(stream, mediaType);
        }

        private Stream GetEmbeddedResourceStreamFor(string path, string rootUrl)
        {
            EmbeddedResourceDescriptor customEmbeddedResource;
            var isCustom = _settings.CustomAssets.TryGetValue(path, out customEmbeddedResource);

            var assembly = isCustom ? customEmbeddedResource.ContainingAssembly : GetType().Assembly;
            var name = isCustom ? customEmbeddedResource.Name : path;

            var stream = assembly.GetManifestResourceStream(name);
            if (stream == null)
                throw new WebAssetNotFound();

            _settings.TemplateValues["%(RootUrl)"] = "'" + rootUrl + "'";

            return isCustom ? stream.FindAndReplace(_settings.TemplateValues) : stream;
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