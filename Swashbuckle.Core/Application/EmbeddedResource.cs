using System.Reflection;
using System.Linq;
using System.IO;

namespace Swashbuckle.Application
{
    internal class EmbeddedResource
    {
        public EmbeddedResource(Assembly assembly, string name, bool supportsConfigExpressions, string mediaType = null)
        {
            ResourceAssembly = assembly;
            ResourceName = name;
            SupportsConfigExpressions = supportsConfigExpressions;
            MediaType = mediaType ?? InferMediaTypeFrom(name);
        }

        public Assembly ResourceAssembly { get; private set; }

        public string ResourceName { get; private set; }

        public bool SupportsConfigExpressions { get; private set; }

        public string MediaType { get; private set; }

        public Stream ToStream()
        {
            var stream = ResourceAssembly.GetManifestResourceStream(ResourceName);
            if (stream == null)
                throw new FileNotFoundException("Ensure the Build Action is set to \"Embedded Resource\"", ResourceName);

            return stream;
        }

        private static string InferMediaTypeFrom(string resourceName)
        {
            var extension = resourceName.Split('.').Last();

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