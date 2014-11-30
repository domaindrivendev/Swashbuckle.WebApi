using System.Collections.Generic;

namespace Swashbuckle.SwaggerUi
{
    public class EmbeddedSwaggerUiProviderSettings
    {
        public EmbeddedSwaggerUiProviderSettings(
            IDictionary<string, EmbeddedAssetDescriptor> customAssets,
            IDictionary<string, string> templateValues)
        {
            CustomAssets = customAssets;
            TemplateValues = templateValues;
        }

        public IDictionary<string, EmbeddedAssetDescriptor> CustomAssets { get; private set; }

        public IDictionary<string, string> TemplateValues { get; private set; }
    }
}
