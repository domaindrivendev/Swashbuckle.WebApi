using System;
using System.Collections.Generic;
using System.Web.Http.Description;

namespace Swashbuckle.Models
{
    public interface IOperationSpecFilter
    {
        void UpdateSpec(ApiDescription apiDescription, ApiOperationSpec operationSpec);
    }

    public class SwaggerGeneratorConfig
    {
        internal static readonly SwaggerGeneratorConfig Instance = new SwaggerGeneratorConfig();

        public static void Customize(Action<SwaggerGeneratorConfig> customize)
        {
            customize(Instance);
        }

        private SwaggerGeneratorConfig()
        {
            PostFilters = new List<IOperationSpecFilter>();
        }

        internal IList<IOperationSpecFilter> PostFilters { get; private set; }

        public void AddFilter(IOperationSpecFilter operationSpecFilter)
        {
            PostFilters.Add(operationSpecFilter);
        }

        public void AddFilter<TFilter>()
            where TFilter : IOperationSpecFilter, new()
        {
            PostFilters.Add(new TFilter());
        }
    }
}