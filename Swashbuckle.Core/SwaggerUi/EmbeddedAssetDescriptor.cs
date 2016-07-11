using System.Reflection;

namespace Swashbuckle.SwaggerUi
{
    public class EmbeddedAssetDescriptor
    {
        public EmbeddedAssetDescriptor(Assembly containingAssembly, string name, bool isTemplate = false, bool isRazorTemplate = false)
        {
            Assembly = containingAssembly;
            Name = name;
            IsTemplate = isTemplate;
            IsRazorTemplate = isRazorTemplate;
        }

        public Assembly Assembly { get; private set; }

        public string Name { get; private set; }

        public bool IsTemplate { get; private set; }

        public bool IsRazorTemplate { get; private set; }
    }
}