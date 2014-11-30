using System.Reflection;

namespace Swashbuckle.SwaggerUi
{
    public class EmbeddedAssetDescriptor
    {
        public EmbeddedAssetDescriptor(Assembly containingAssembly, string name)
        {
            ContainingAssembly = containingAssembly;
            Name = name;
        }

        public Assembly ContainingAssembly { get; private set; }

        public string Name { get; private set; }
    }
}