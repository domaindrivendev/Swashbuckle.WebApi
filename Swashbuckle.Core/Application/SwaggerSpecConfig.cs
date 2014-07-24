using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Web.Http.Description;
using System.Linq;
using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    public class SwaggerSpecConfig
    {
        internal static readonly SwaggerSpecConfig StaticInstance = new SwaggerSpecConfig();

        public static void Customize(Action<SwaggerSpecConfig> customize)
        {
            customize(StaticInstance);
        }

        internal Func<HttpRequestMessage, string> BasePathResolver { get; set; }
        internal Func<HttpRequestMessage, string> TargetVersionResolver { get; set; }
        internal Info Info { get; set; }
        internal Dictionary<string, Authorization> Authorizations { get; set; }

        private bool _ignoreObsoleteActions;
        private Func<ApiDescription, string, bool> _versionSupportResolver;
        private Func<ApiDescription, string> _resourceNameResolver;
        private readonly Dictionary<Type, Func<DataType>> _customTypeMappings;
        private readonly List<PolymorphicType> _polymorphicTypes;

        private readonly List<Func<IModelFilter>> _modelFilterFactories;
        private readonly List<Func<IOperationFilter>> _operationFilterFactories;
        
        public SwaggerSpecConfig()
        {
            BasePathResolver = (req) => req.RequestUri.GetLeftPart(UriPartial.Authority) + req.GetConfiguration().VirtualPathRoot.TrimEnd('/');
            TargetVersionResolver = (req) => "1.0";

            _ignoreObsoleteActions = false;
            _versionSupportResolver = (apiDesc, version) => true;
            _resourceNameResolver = (apiDesc) => apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName;
            _customTypeMappings = new Dictionary<Type, Func<DataType>>();
            _polymorphicTypes = new List<PolymorphicType>();

            _modelFilterFactories = new List<Func<IModelFilter>>();
            _operationFilterFactories = new List<Func<IOperationFilter>>();
        }

        public SwaggerSpecConfig ResolveBasePathUsing(Func<HttpRequestMessage, string> basePathResolver)
        {
            if (basePathResolver == null) throw new ArgumentNullException("basePathResolver");
            BasePathResolver = basePathResolver;
            return this;
        }

        public SwaggerSpecConfig ResolveTargetVersionUsing(Func<HttpRequestMessage, string> targetVersionResolver)
        {
            if (targetVersionResolver == null) throw new ArgumentNullException("targetVersionResolver");
            TargetVersionResolver = targetVersionResolver;
            return this;
        }

        public SwaggerSpecConfig IgnoreObsoleteActions()
        {
            _ignoreObsoleteActions = true;
            return this;
        }

        public SwaggerSpecConfig ApiInfo(Info info)
        {
            this.Info = info;
            return this;
        }

        public SwaggerSpecConfig SetOAuth2Authorization(Authorization auth)
        {
            if (this.Authorizations == null)
                this.Authorizations = new Dictionary<string,Authorization>();

            this.Authorizations["oauth2"] = auth;
            return this;
        }

        public SwaggerSpecConfig ResolveVersionSupportUsing(Func<ApiDescription, string, bool> versionSupportResolver)
        {
            if (versionSupportResolver == null) throw new ArgumentNullException("versionSupportResolver");
            _versionSupportResolver = versionSupportResolver;
            return this;
        }

        public SwaggerSpecConfig GroupDeclarationsBy(Func<ApiDescription, string> resourceNameResolver)
        {
            if (resourceNameResolver == null) throw new ArgumentNullException("resourceNameResolver");
            _resourceNameResolver = resourceNameResolver;
            return this;
        }

        public SwaggerSpecConfig MapType<T>(Func<DataType> factory)
        {
            _customTypeMappings[typeof (T)] = factory;
            return this;
        }

        public SwaggerSpecConfig PolymorphicType<TBase>(Action<PolymorphicBaseType<TBase>> configure)
        {
            var polymorphicType = new PolymorphicBaseType<TBase>();
            configure(polymorphicType);
            _polymorphicTypes.Add(polymorphicType);
            return this;
        }

        public SwaggerSpecConfig ModelFilter<T>()
            where T : IModelFilter, new()
        {
            return ModelFilter(new T());
        }

        public SwaggerSpecConfig ModelFilter(IModelFilter modelFilter)
        {
            if (modelFilter == null) throw new ArgumentNullException("modelFilter");
            _modelFilterFactories.Add(() => modelFilter);
            return this;
        }

        public SwaggerSpecConfig OperationFilter<T>()
            where T : IOperationFilter, new()
        {
            return OperationFilter(new T());
        }

        public SwaggerSpecConfig OperationFilter(IOperationFilter operationFilter)
        {
            if (operationFilter == null) throw new ArgumentNullException("operationFilter");
            _operationFilterFactories.Add(() => operationFilter);
            return this;
        }

        public SwaggerSpecConfig IncludeXmlComments(string xmlCommentsPath)
        {
            _operationFilterFactories.Add(() => new ApplyActionXmlComments(xmlCommentsPath));
            _modelFilterFactories.Add(() => new ApplyTypeXmlComments(xmlCommentsPath));
            return this;
        }

        internal ISwaggerProvider GetSwaggerProvider(IApiExplorer apiExplorer)
        {
            var modelFilters = _modelFilterFactories.Select((f) => f());
            var operationFilters = _operationFilterFactories.Select((f) => f());

            return new ApiExplorerAdapter(
                apiExplorer,
                _ignoreObsoleteActions,
                _versionSupportResolver,
                _resourceNameResolver,
                _customTypeMappings,
                _polymorphicTypes,
                modelFilters,
                operationFilters,
                this.Info,
                this.Authorizations);
        }
    }
}