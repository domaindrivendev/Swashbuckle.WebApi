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
            GroupingStrategy = new ControllerGroupingStrategy();
            PostFilters = new List<IOperationSpecFilter>();
        }

        internal IGroupingStrategy GroupingStrategy { get; private set; }

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

    public interface IGroupingStrategy
    {
        string GetKeyFrom(ApiDescription apiDescription);
    }

    public interface IOperationSpecFilter
    {
        void Apply(ApiDescription apiDescription, OperationSpec operationSpec);
    }

    public class ControllerGroupingStrategy : IGroupingStrategy
    {
        public string GetKeyFrom(ApiDescription apiDescription)
        {
            return apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName;
        }
    }
}