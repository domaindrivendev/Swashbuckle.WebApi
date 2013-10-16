using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Description;

namespace Swashbuckle.Models
{
    public interface IOperationSpecFilter
    {
        void Apply(ApiDescription apiDescription, OperationSpec operationSpec);
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
        }

        internal Func<ApiDescription, string> DeclarationKeySelector { get; set; }
        internal Func<HttpRequestMessage, string> BasePathResolver { get; private set; }
        internal ICollection<IOperationSpecFilter> OperationSpecFilters { get; private set; }

        public void GroupDeclarationsBy(Func<ApiDescription, string> declarationKeySelector)
        {
            if (declarationKeySelector == null)
                throw new ArgumentNullException("declarationKeySelector");
            DeclarationKeySelector = declarationKeySelector;
        }

        public void ResolveBasePath(Func<HttpRequestMessage, string> resolver)
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

        private string DefaultDeclarationKeySelector(ApiDescription apiDescription)
        {
            return apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName;
        }

        private string DefaultBasePathResolver(HttpRequestMessage request)
        {
            return request.RequestUri.GetLeftPart(UriPartial.Authority) + request.GetConfiguration().VirtualPathRoot;
        }
    }
}