using System;
using System.Collections.Generic;
using System.Web.Http.Description;

namespace Swashbuckle.Models
{
    public interface IOperationSpecFilter
    {
        void Apply(ApiDescription apiDescription, ApiOperationSpec operationSpec);
    }

    public class SwaggerSpecConfig
    {
        internal static readonly SwaggerSpecConfig Instance = new SwaggerSpecConfig();

        public static void Customize(Action<SwaggerSpecConfig> customize)
        {
            customize(Instance);
        }

        private SwaggerSpecConfig()
        {
            PostFilters = new List<IOperationSpecFilter>();
        }

        internal ICollection<IOperationSpecFilter> PostFilters { get; private set; }

        public void PostFilter(IOperationSpecFilter operationSpecFilter)
        {
            PostFilters.Add(operationSpecFilter);
        }

        public void PostFilter<TFilter>()
            where TFilter : IOperationSpecFilter, new()
        {
            PostFilters.Add(new TFilter());
        }
    }
}