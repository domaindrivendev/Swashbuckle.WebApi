using System.Collections.Generic;

namespace Swashbuckle.WebAssets
{
    public class EmbeddedWebAssetProviderSettings
    {
        public EmbeddedWebAssetProviderSettings(
            IDictionary<string, EmbeddedResourceDescriptor> customAssets,
            IDictionary<string, string> textReplacements)
        {
            CustomAssets = customAssets;
            TextReplacements = textReplacements;
        }

        public IDictionary<string, EmbeddedResourceDescriptor> CustomAssets { get; private set; }

        public IDictionary<string, string> TextReplacements { get; private set; }
    }
}
