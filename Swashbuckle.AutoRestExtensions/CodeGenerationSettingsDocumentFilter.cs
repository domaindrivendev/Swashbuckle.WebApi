using System;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Swashbuckle.AutoRestExtensions
{
    public class CodeGenerationSettingsDocumentFilter : IDocumentFilter
    {
        public CodeGenerationSettingsDocumentFilter(object settings) // Pass an anonymous type for easy opt-in
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            _settings = settings;
        }

        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            // Add code generation settings
            swaggerDoc.info.vendorExtensions.Add("x-ms-code-generation-settings", _settings);
        }

        private readonly object _settings;
    }
}
