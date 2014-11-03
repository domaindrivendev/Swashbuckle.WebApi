using System.Collections.Generic;

namespace Swashbuckle.WebAssets
{
    public class EmbeddedWebAssetProviderSettings
    {
        public EmbeddedWebAssetProviderSettings(
            IDictionary<string, EmbeddedResourceDescriptor> customAssets,
            IDictionary<string, string> templateValues)
        {
            CustomAssets = customAssets;
            TemplateValues = templateValues;
        }

        public IDictionary<string, EmbeddedResourceDescriptor> CustomAssets { get; private set; }

        public IDictionary<string, string> TemplateValues { get; private set; }
    }
}
