using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;

namespace Swashbuckle.Models
{
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
            GroupingStrategy = new ControllerGroupingStrategy();
        }

        internal IApiGroupingStrategy GroupingStrategy { get; private set; }

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

    public interface IApiGroupingStrategy
    {
        IEnumerable<IGrouping<string, ApiDescription>> Group(IEnumerable<ApiDescription> apiDescriptions);
    }

    public interface IOperationSpecFilter
    {
        void Apply(ApiDescription apiDescription, ApiOperationSpec operationSpec);
    }

    public class ControllerGroupingStrategy : IApiGroupingStrategy
    {
        public IEnumerable<IGrouping<string, ApiDescription>> Group(IEnumerable<ApiDescription> apiDescriptions)
        {
            return apiDescriptions.GroupBy(ad => ad.ActionDescriptor.ControllerDescriptor.ControllerName);
        }
    }
}