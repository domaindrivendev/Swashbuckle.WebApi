using System;
using System.Collections.Generic;
using System.Web.Http.Description;

namespace Swashbuckle.Models
{
    public class SwaggerGeneratorConfig
    {
        private readonly List<IOperationSpecFilter> _operationSpecFilters = new List<IOperationSpecFilter>();

        public SwaggerGeneratorConfig UseApiExplorer(IApiExplorer apiExplorer)
        {
            ApiExplorer = apiExplorer;
            return this;
        }

        public SwaggerGeneratorConfig UseBasePath(Func<string> basPathAccessor)
        {
            BasePathAccessor = basPathAccessor;
            return this;
        }

        public SwaggerGeneratorConfig AddOperationSpecFilter(IOperationSpecFilter operationSpecFilter)
        {
            _operationSpecFilters.Add(operationSpecFilter);
            return this;
        }

        internal Func<string> BasePathAccessor { get; private set; }

        internal IApiExplorer ApiExplorer { get; private set; }

        internal IEnumerable<IOperationSpecFilter> OperationSpecFilters
        {
            get { return _operationSpecFilters; }
        }
    }
}