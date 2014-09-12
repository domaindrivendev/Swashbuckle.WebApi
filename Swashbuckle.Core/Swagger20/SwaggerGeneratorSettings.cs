using System.Collections.Generic;

namespace Swashbuckle.Swagger20
{
    public class SwaggerGeneratorSettings
    {
        public SwaggerGeneratorSettings(
            Info info,
            string host,
            string virtualPathRoot,
            IEnumerable<string> schemes,
            IEnumerable<IOperationFilter> operationFilters = null,
            IEnumerable<IDocumentFilter> documentFilters = null)
        {
            Info = info;
            Host = host;
            VirtualPathRoot = virtualPathRoot;
            Schemes = schemes;
            OperationFilters = operationFilters ?? new List<IOperationFilter>();
            DocumentFilters = documentFilters ?? new List<IDocumentFilter>();
        }

        public Info Info { get; private set; }

        public string Host { get; private set; }

        public string VirtualPathRoot { get; private set; }

        public IEnumerable<string> Schemes { get; private set; }

        public IEnumerable<IOperationFilter> OperationFilters { get; private set; }

        public IEnumerable<IDocumentFilter> DocumentFilters { get; private set; }
    }
}