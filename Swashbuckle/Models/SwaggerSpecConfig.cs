using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Http.Description;

namespace Swashbuckle.Models
{
    public class SwaggerSpecConfig
    {
        internal static readonly SwaggerSpecConfig StaticInstance = new SwaggerSpecConfig();

        public static void Customize(Action<SwaggerSpecConfig> customize)
        {
            customize(StaticInstance);
        }

        public SwaggerSpecConfig()
        {
            IgnoreObsoleteActions = false;
            ApiVersion = "1.0";
            BasePathResolver = DefaultBasePathResolver;
            DeclarationKeySelector = DefaultDeclarationKeySelector;
            CustomTypeMappings = new Dictionary<Type, ModelSpec>();
            SubTypesLookup = new Dictionary<Type, IEnumerable<Type>>();
            OperationFilters = new List<IOperationFilter>();
            OperationSpecFilters = new List<IOperationSpecFilter>();
        }

        public bool IgnoreObsoleteActions { get; set; }
        public string ApiVersion { get; set; }
        internal Func<string> BasePathResolver { get; private set; }
        internal Func<ApiDescription, string> DeclarationKeySelector { get; private set; }
        internal IDictionary<Type, ModelSpec> CustomTypeMappings { get; private set; }
        internal Dictionary<Type, IEnumerable<Type>> SubTypesLookup { get; set; }
        internal List<IOperationFilter> OperationFilters { get; private set; }
        internal List<IOperationSpecFilter> OperationSpecFilters { get; private set; }

        public SwaggerSpecConfig ResolveBasePath(Func<string> basePathResolver)
        {
            if (basePathResolver == null)
                throw new ArgumentNullException("basePathResolver");
            BasePathResolver = basePathResolver;
            return this;
        }

        public SwaggerSpecConfig GroupDeclarationsBy(Func<ApiDescription, string> declarationKeySelector)
        {
            if (declarationKeySelector == null)
                throw new ArgumentNullException("declarationKeySelector");
            DeclarationKeySelector = declarationKeySelector;
            return this;
        }

        public SwaggerSpecConfig MapType<T>(ModelSpec modelSpec)
        {
            CustomTypeMappings[typeof (T)] = modelSpec;
            return this;
        }

        public SubTypeList<TBase> SubTypesOf<TBase>(params Type[] subTypes)
        {
            var baseType = typeof (TBase);
            IEnumerable<Type> subTypeList;

            if (!SubTypesLookup.TryGetValue(baseType, out subTypeList))
            {
                subTypeList = new SubTypeList<TBase>();
                SubTypesLookup.Add(baseType, subTypeList);
            }

            return (SubTypeList<TBase>)subTypeList;
        }

        public SwaggerSpecConfig OperationFilter(IOperationFilter operationFilter)
        {
            OperationFilters.Add(operationFilter);
            return this;
        }

        public SwaggerSpecConfig OperationFilter<TFilter>()
            where TFilter : IOperationFilter, new()
        {
            return OperationFilter(new TFilter());
        }

        [Obsolete("Use OperationFilter and port any custom filters from IOperationSpecFilter to IOperationFilter")]
        public SwaggerSpecConfig PostFilter(IOperationSpecFilter operationSpecFilter)
        {
            OperationSpecFilters.Add(operationSpecFilter);
            return this;
        }

        [Obsolete("Use OperationFilter and port any custom filters from IOperationSpecFilter to IOperationFilter")]
        public SwaggerSpecConfig PostFilter<TFilter>()
            where TFilter : IOperationSpecFilter, new()
        {
            return PostFilter(new TFilter());
        }

        private static string DefaultDeclarationKeySelector(ApiDescription apiDescription)
        {
            return apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName;
        }

        private static string DefaultBasePathResolver()
        {
            return HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + HttpRuntime.AppDomainAppVirtualPath;
        }
    }

    public class SubTypeList<TBase> : IEnumerable<Type>
    {
        readonly List<Type> _subTypes = new List<Type>();

        public SubTypeList<TBase> Include<T>()
            where T : TBase
        {
            var type = typeof (T);
            if (!_subTypes.Contains(type))
                _subTypes.Add(type);

            return this;
        }

        public IEnumerator<Type> GetEnumerator()
        {
            return _subTypes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _subTypes.GetEnumerator();
        }
    }

    public interface IOperationFilter
    {
        void Apply(
            ApiDescription apiDescription,
            OperationSpec operationSpec,
            ModelSpecRegistrar modelSpecRegistrar,
            ModelSpecGenerator modelSpecGenerator);
    }

    [Obsolete("Use new interface - IOperationFilter. It provides additional parameters for generating/registering custom ModelSpecs")]
    public interface IOperationSpecFilter
    {
        void Apply(ApiDescription apiDescription, OperationSpec operationSpec, ModelSpecMap modelSpecMap);
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
            IEnumerable<ModelSpec> complexSpecs;
            var modelSpec = _modelSpecGenerator.TypeToModelSpec(type, out complexSpecs);

            _modelSpecRegistrar.RegisterMany(complexSpecs);

            if (modelSpec.Type == "object")
                _modelSpecRegistrar.Register(modelSpec);

            return modelSpec;
        }
    }
}