using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Swashbuckle.SwaggerUi
{
    public class EmbeddedSwaggerUiProvider : ISwaggerUiProvider
    {
        private readonly EmbeddedSwaggerUiProviderSettings _settings;
        private readonly Dictionary<string, string> _templateValues;

        public EmbeddedSwaggerUiProvider(
            string rootUrl,
            EmbeddedSwaggerUiProviderSettings settings)
        {
            _settings = settings;

            // By default, add RootUrl to template values
            _templateValues = _settings.TemplateValues
                .Union(new Dictionary<string, string> { { "%(RootUrl)", "'" + rootUrl + "'" } })
                .ToDictionary(entry => entry.Key, entry => entry.Value);
        }

        public Asset GetAssetFor(string path)
        {
            var stream = GetEmbeddedResourceStreamFor(path);
            var mediaType = InferMediaTypeFrom(path);
            return new Asset(stream, mediaType);
        }

        private Stream GetEmbeddedResourceStreamFor(string assetPath)
        {
            EmbeddedAssetDescriptor customEmbeddedResource;
            var isCustom = _settings.CustomAssets.TryGetValue(assetPath, out customEmbeddedResource);

            var assembly = isCustom ? customEmbeddedResource.ContainingAssembly : GetType().Assembly;
            var name = isCustom ? customEmbeddedResource.Name : assetPath;

            var stream = assembly.GetManifestResourceStream(name);
            if (stream == null)
                throw new AssetNotFound();

            return isCustom ? stream.FindAndReplace(_templateValues) : stream;
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