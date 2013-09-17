using System;
using System.Collections.Generic;
using System.Web;
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
            BasePathResolver = DefaultBasePathResolver;
        }

        internal ICollection<IOperationSpecFilter> PostFilters { get; private set; }
        internal Func<string> BasePathResolver { get; private set; } 

        public void PostFilter(IOperationSpecFilter operationSpecFilter)
        {
            PostFilters.Add(operationSpecFilter);
        }

        public void PostFilter<TFilter>()
            where TFilter : IOperationSpecFilter, new()
        {
            PostFilters.Add(new TFilter());
        }

        public void ResolveBasePath(Func<string> resolver)
        {
            if(resolver == null)
                throw new ArgumentNullException("resolver");
            BasePathResolver = resolver;
        }

        private string DefaultBasePathResolver()
        {
            return HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + HttpRuntime.AppDomainAppVirtualPath;
        }
    }
}