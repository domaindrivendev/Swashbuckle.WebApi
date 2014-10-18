using System.Reflection;

namespace Swashbuckle.WebAssets
{
    public class EmbeddedResourceDescriptor
    {
        public EmbeddedResourceDescriptor(Assembly containingAssembly, string name)
        {
            ContainingAssembly = containingAssembly;
            Name = name;
        }

        public Assembly ContainingAssembly { get; private set; }

        public string Name { get; private set; }
    }
}