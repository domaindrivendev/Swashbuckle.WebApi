using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Description;

namespace Swashbuckle.Core.Models
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
            DeclarationKeySelector = DefaultDeclarationKeySelector;
            BasePathResolver = DefaultBasePathResolver;
            CustomTypeMappings = new Dictionary<Type, ModelSpec>();
            OperationFilters = new List<IOperationFilter>();
            OperationSpecFilters = new List<IOperationSpecFilter>();
        }

        internal Func<ApiDescription, string> DeclarationKeySelector { get; private set; }
        internal Func<HttpRequestMessage, string> BasePathResolver { get; private set; }
        internal IDictionary<Type,ModelSpec> CustomTypeMappings { get; private set; }
        internal List<IOperationFilter> OperationFilters { get; private set; }
        internal List<IOperationSpecFilter> OperationSpecFilters { get; private set; }

        public void GroupDeclarationsBy(Func<ApiDescription, string> declarationKeySelector)
        {
            if (declarationKeySelector == null)
                throw new ArgumentNullException("declarationKeySelector");
            DeclarationKeySelector = declarationKeySelector;
        }

        public void ResolveBasePath(Func<HttpRequestMessage, string> basePathResolver)
        {
            if(basePathResolver == null)
                throw new ArgumentNullException("basePathResolver");
            BasePathResolver = basePathResolver;
        }

        public void OperationFilter(IOperationFilter operationFilter)
        {
            OperationFilters.Add(operationFilter);
        }

        public void OperationFilter<TFilter>()
            where TFilter : IOperationFilter, new()
        {
            OperationFilters.Add(new TFilter());
        }

        [Obsolete("Use OperationFilter and port any custom filters from IOperationSpecFilter to IOperationFilter")]
        public void PostFilter(IOperationSpecFilter operationSpecFilter)
        {
            OperationSpecFilters.Add(operationSpecFilter);
        }

        [Obsolete("Use OperationFilter and port any custom filters from IOperationSpecFilter to IOperationFilter")]
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

        private string DefaultBasePathResolver(HttpRequestMessage request)
        {
            var url = request.RequestUri.ToString();
            var cutoffIndex = url.IndexOf("/swagger", StringComparison.InvariantCulture);

            return url.Substring(0, cutoffIndex);
        }
    }

    [Obsolete("Use new interface - IOperationFilter. It provides additional parameters for generating/registering custom ModelSpecs")]
    public interface IOperationSpecFilter
    {
        void Apply(ApiDescription apiDescription, OperationSpec operationSpec, ModelSpecMap modelSpecMap);
    }

    public interface IOperationFilter
    {
        void Apply(
            ApiDescription apiDescription,
            OperationSpec operationSpec,
            ModelSpecRegistrar modelSpecRegistrar,
            ModelSpecGenerator modelSpecGenerator);
    }

    public class ModelSpecMap
    {
        private readonly ModelSpecRegistrar _modelSpecRegistrar;
        private readonly ModelSpecGenerator _modelSpecGenerator;

        public ModelSpecMap(ModelSpecRegistrar modelSpecRegistrar, ModelSpecGenerator modelSpecGenerator)
        {
            _modelSpecRegistrar = modelSpecRegistrar;
            _modelSpecGenerator = modelSpecGenerator;
        }

        public ModelSpec FindOrCreateFor(Type type)
        {
            return _modelSpecGenerator.TypeToModelSpec(type, _modelSpecRegistrar);
        }
    }
}