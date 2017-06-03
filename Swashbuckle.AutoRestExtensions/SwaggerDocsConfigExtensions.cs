using Swashbuckle.Application;

namespace Swashbuckle.AutoRestExtensions
{
    public static class SwaggerDocsConfigExtensions
    {
        public static void ApplyAutoRestFilters(this SwaggerDocsConfig config, object codeGenerationSettings = null, bool enumTypeModelAsString = false, bool nonNullableAsRequired = false)
        {
            if (codeGenerationSettings != null)
                config.DocumentFilter(() => new CodeGenerationSettingsDocumentFilter(codeGenerationSettings));
            config.SchemaFilter(() => new EnumTypeSchemaFilter(enumTypeModelAsString));
            config.SchemaFilter<TypeFormatSchemaFilter>();
            config.SchemaFilter<NullableTypeSchemaFilter>();
            if (nonNullableAsRequired)
                config.SchemaFilter<NonNullableAsRequiredSchemaFilter>();
            config.ApplyFiltersToAllSchemas();
        }
    }
}
