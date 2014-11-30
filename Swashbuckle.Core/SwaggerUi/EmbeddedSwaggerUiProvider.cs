using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Swashbuckle.SwaggerUi
{
    public class EmbeddedSwaggerUiProvider : ISwaggerUiProvider
    {
        private readonly EmbeddedSwaggerUiProviderSettings _settings;

        public EmbeddedSwaggerUiProvider(
            string rootUrl,
            EmbeddedSwaggerUiProviderSettings settings)
        {
            _settings = settings;
            _settings.TemplateValues["%(RootUrl)"] = "'" + rootUrl + "'";
        }

        public Asset GetAssetFor(string path)
        {
            var stream = GetEmbeddedResourceStreamFor(path);
            var mediaType = InferMediaTypeFrom(path);
            return new Asset(stream, mediaType);
        }

        private Stream GetEmbeddedResourceStreamFor(string path)
        {
            EmbeddedAssetDescriptor customEmbeddedResource;
            var isCustom = _settings.CustomAssets.TryGetValue(path, out customEmbeddedResource);

            var assembly = isCustom ? customEmbeddedResource.ContainingAssembly : GetType().Assembly;
            var name = isCustom ? customEmbeddedResource.Name : path;

            var stream = assembly.GetManifestResourceStream(name);
            if (stream == null)
                throw new AssetNotFound();

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