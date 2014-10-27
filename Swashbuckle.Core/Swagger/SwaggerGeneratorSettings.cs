using System;
using System.Collections.Generic;
using System.Web.Http.Description;

namespace Swashbuckle.Swagger
{
    public class SwaggerGeneratorSettings
    {
        public SwaggerGeneratorSettings(
            Func<ApiDescription, string, bool> versionSupportResolver,
            IDictionary<string, Info> apiVersions,
            IEnumerable<string> schemes = null,
            IDictionary<string, SecurityScheme> securityDefinitions = null,
            IEnumerable<ISchemaFilter> schemaFilters = null,
            IEnumerable<IOperationFilter> operationFilters = null,
            IEnumerable<IDocumentFilter> documentFilters = null)
        {
            VersionSupportResolver = versionSupportResolver;
            ApiVersions = apiVersions;
            Schemes = schemes;
            SecurityDefinitions = securityDefinitions;
            SchemaFilters = schemaFilters ?? new List<ISchemaFilter>();
            OperationFilters = operationFilters ?? new List<IOperationFilter>();
            DocumentFilters = documentFilters ?? new List<IDocumentFilter>();
        }

        public Func<ApiDescription, string, bool> VersionSupportResolver { get; private set; }

        public IDictionary<string, Info> ApiVersions { get; private set; }

        public IEnumerable<string> Schemes { get; private set; }

        public IDictionary<string, SecurityScheme> SecurityDefinitions { get; private set; }

        public IEnumerable<ISchemaFilter> SchemaFilters { get; private set; }

        public IEnumerable<IOperationFilter> OperationFilters { get; private set; }

        public IEnumerable<IDocumentFilter> DocumentFilters { get; private set; }

    }
}