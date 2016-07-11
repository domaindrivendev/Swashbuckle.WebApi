using System;
using System.IO;

namespace Swashbuckle.SwaggerUi
{
    public interface IAssetProvider
    {
        Asset GetAsset(string rootUrl, string assetPath);
    }

    public class Asset
    {
        public Asset(Stream stream, string mediaType, bool disableCache = false)
        {
            Stream = stream;
            MediaType = mediaType;
            DisableClientCache = disableCache;
        }

        public Stream Stream { get; private set; }

        public string MediaType { get; private set; }
        public bool DisableClientCache { get; set; }
    }

    public class AssetNotFound : Exception
    {
        public AssetNotFound(string message)
            : base(message)
        {}
    }
}
