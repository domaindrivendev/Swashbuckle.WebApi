using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http.Description;

namespace Swashbuckle.Models
{
    public interface IOperationSpecFilter
    {
        void Apply(ApiDescription apiDescription, OperationSpec operationSpec, ModelSpecMap modelSpecMap);
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
            DeclarationKeySelector = DefaultDeclarationKeySelector;
            BasePathResolver = DefaultBasePathResolver;
            OperationSpecFilters = new List<IOperationSpecFilter>();
            CustomTypeMappings = new Dictionary<Type, ModelSpec>();
        }

        internal Func<ApiDescription, string> DeclarationKeySelector { get; set; }
        internal Func<string> BasePathResolver { get; private set; }
        internal ICollection<IOperationSpecFilter> OperationSpecFilters { get; private set; }
        internal IDictionary<Type,ModelSpec> CustomTypeMappings { get; private set; } 

        public void GroupDeclarationsBy(Func<ApiDescription, string> declarationKeySelector)
        {
            if (declarationKeySelector == null)
                throw new ArgumentNullException("declarationKeySelector");
            DeclarationKeySelector = declarationKeySelector;
        }

        public void ResolveBasePath(Func<string> resolver)
        {
            if(resolver == null)
                throw new ArgumentNullException("resolver");
            BasePathResolver = resolver;
        }

        public void PostFilter(IOperationSpecFilter operationSpecFilter)
        {
            OperationSpecFilters.Add(operationSpecFilter);
        }

        public void PostFilter<TFilter>()
            where TFilter : IOperationSpecFilter, new()
        {
            OperationSpecFilters.Add(new TFilter());
        }

        public void MapType<T>(ModelSpec modelSpec)
        {
            CustomTypeMappings[typeof (T)] = modelSpec;
        }

        private string DefaultDeclarationKeySelector(ApiDescription apiDescription)
        {
            return apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName;
        }

        private string DefaultBasePathResolver()
        {
            return HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + HttpRuntime.AppDomainAppVirtualPath;
        }
    }
}