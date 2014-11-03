using System;
using System.IO;

namespace Swashbuckle.WebAssets
{
    public interface IWebAssetProvider
    {
        WebAsset GetWebAssetFor(string path, string rootUrl);
    }

    public class WebAsset
    {
        public WebAsset(Stream stream, string mediaType)
        {
            Stream = stream;
            MediaType = mediaType;
        }

        public Stream Stream { get; private set; }

        public string MediaType { get; private set; }
    }

    public class WebAssetNotFound : Exception
    {}
}
