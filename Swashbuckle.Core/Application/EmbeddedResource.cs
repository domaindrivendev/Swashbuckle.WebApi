using System.Reflection;
using System.Linq;
using System.IO;

namespace Swashbuckle.Application
{
    internal class EmbeddedResource
    {
        public EmbeddedResource(Assembly assembly, string name, string mediaType = null)
        {
            Assembly = assembly;
            Name = name;
            MediaType = mediaType ?? InferMediaTypeFrom(name);
        }

        public Assembly Assembly { get; private set; }

        public string Name { get; private set; }

        public string MediaType { get; private set; }

        public Stream GetStream()
        {
            var stream = Assembly.GetManifestResourceStream(Name);
            if (stream == null)
                throw new FileNotFoundException("Ensure the Build Action is set to \"Embedded Resource\"", Name);

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