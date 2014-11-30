using System;
using System.IO;

namespace Swashbuckle.SwaggerUi
{
    public interface ISwaggerUiProvider
    {
        Asset GetAssetFor(string path);
    }

    public class Asset
    {
        public Asset(Stream stream, string mediaType)
        {
            Stream = stream;
            MediaType = mediaType;
        }

        public Stream Stream { get; private set; }

        public string MediaType { get; private set; }
    }

    public class AssetNotFound : Exception
    {}
}
