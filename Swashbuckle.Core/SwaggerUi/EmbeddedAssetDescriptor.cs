using System.Reflection;

namespace Swashbuckle.SwaggerUi
{
    public class EmbeddedAssetDescriptor
    {
        public EmbeddedAssetDescriptor(Assembly containingAssembly, string name, bool isTemplate = false)
        {
            Assembly = containingAssembly;
            Name = name;
            IsTemplate = isTemplate;
        }

        public Assembly Assembly { get; private set; }

        public string Name { get; private set; }

        public bool IsTemplate { get; private set; }
    }
}