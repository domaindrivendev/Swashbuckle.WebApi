using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Linq;
using Swashbuckle.Swagger;
using Swashbuckle.SwaggerExtensions;

namespace Swashbuckle.Application
{
    public class SwaggerSpecConfig
    {
        internal static readonly SwaggerSpecConfig StaticInstance = new SwaggerSpecConfig();

        public static void Customize(Action<SwaggerSpecConfig> customize)
        {
            customize(StaticInstance);
        }

        private Func<HttpRequestMessage, string> _targetVersionResolver; // obsolete

        private bool _ignoreObsoleteActions;
        private Func<ApiDescription, string, bool> _versionSupportResolver;

        private Func<ApiDescription, string> _declarationNameResolver;
        private IComparer<string> _resourceNameComparer;
        private readonly Dictionary<Type, Func<DataType>> _customTypeMappings;
        private readonly List<PolymorphicType> _polymorphicTypes;
        private readonly List<Func<IModelFilter>> _modelFilterFactories;
        private readonly List<Func<IOperationFilter>> _operationFilterFactories;
        private Func<ApiDescription, bool> _shouldIgnoreResolver;

        private Info _apiInfo;
        private IDictionary<string, Authorization> _authorizations { get; set; }


        public SwaggerSpecConfig()
        {
            BasePathResolver = (req) => req.RequestUri.GetLeftPart(UriPartial.Authority) + req.GetConfiguration().VirtualPathRoot.TrimEnd('/');
            _shouldIgnoreResolver = (desc) => false;
            Versions = null; 

            _targetVersionResolver = (req) => "1.0"; // obsolete
            _ignoreObsoleteActions = false;
            _versionSupportResolver = (apiDesc, version) => true;

            _declarationNameResolver = (apiDesc) => apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName;
            _resourceNameComparer = Comparer<string>.Default;
            _customTypeMappings = new Dictionary<Type, Func<DataType>>();
            _polymorphicTypes = new List<PolymorphicType>();
            _modelFilterFactories = new List<Func<IModelFilter>>();
            _operationFilterFactories = new List<Func<IOperationFilter>>();
        }

        internal Func<HttpRequestMessage, string> BasePathResolver { get; private set; }

        internal IEnumerable<string> Versions { get; private set; }

        public SwaggerSpecConfig ResolveBasePathUsing(Func<HttpRequestMessage, string> basePathResolver)
        {
            if (basePathResolver == null)
                throw new ArgumentNullException("basePathResolver");
            BasePathResolver = basePathResolver;
            return this;
        }

        public SwaggerSpecConfig IgnoreActionsWhere(Func<ApiDescription, bool> ignoreResolver)
        {
            if (ignoreResolver == null)
                throw new ArgumentNullException("ignoreResolver");
            _shouldIgnoreResolver = ignoreResolver;
            return this;
        }

        [Obsolete("Use ApiVersion OR, if you want to document multiple API versions, SupportMultipleApiVersions")]
        public SwaggerSpecConfig ResolveTargetVersionUsing(Func<HttpRequestMessage, string> targetVersionResolver)
        {
            if (targetVersionResolver == null)
                throw new ArgumentNullException("targetVersionResolver");
            _targetVersionResolver = targetVersionResolver;
            return this;
        }

        [Obsolete("Use SupportMultipleApiVersions if you want to document multiple API versions")]
        public SwaggerSpecConfig ResolveVersionSupportUsing(Func<ApiDescription, string, bool> versionSupportResolver)
        {
            if (versionSupportResolver == null)
                throw new ArgumentNullException("versionSupportResolver");
            _versionSupportResolver = versionSupportResolver;
            return this;
        }

        public SwaggerSpecConfig ApiVersion(string apiVersion)
        {
            Versions = null;
            _targetVersionResolver = (req) => apiVersion;
            _versionSupportResolver = (apiDesc, version) => true;
            return this;
        }

        public SwaggerSpecConfig IgnoreObsoleteActions()
        {
            _ignoreObsoleteActions = true;
            return this;
        }

        public SwaggerSpecConfig SupportMultipleApiVersions(
            IEnumerable<string> versions,
            Func<ApiDescription, string, bool> versionSupportResolver)
        {
            if (versions == null)
                throw new ArgumentNullException("versions");
            if (!versions.Any())
                throw new ArgumentException("one or more versions must be provided");
            Versions = versions;

            _targetVersionResolver = (req) => req.GetRouteData().Values["apiVersion"].ToString();

            if (versionSupportResolver == null)
                throw new ArgumentNullException("versionSupportResolver");
            _versionSupportResolver = versionSupportResolver;

            return this;
        }

        public SwaggerSpecConfig GroupDeclarationsBy(Func<ApiDescription, string> declarationNameResolver)
        {
            if (declarationNameResolver == null)
                throw new ArgumentNullException("declarationNameResolver");
            _declarationNameResolver = declarationNameResolver;
            return this;
        }

        public SwaggerSpecConfig SortDeclarationsBy(IComparer<string> resourceNameComparer)
        {
            if (resourceNameComparer == null) throw new ArgumentNullException("resourceNameComparer");
            _resourceNameComparer = resourceNameComparer;
            return this;
        }

        public SwaggerSpecConfig MapType<T>(Func<DataType> factory)
        {
            _customTypeMappings[typeof(T)] = factory;
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

        public SwaggerSpecConfig IgnoreObsoleteModelFields()
        {
            return ModelFilter<HideObsoleteModelFields>();
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
            return ModelFilter(new ApplyTypeXmlComments(xmlCommentsPath));
        }

        public SwaggerSpecConfig ApiInfo(Info apiInfo)
        {
            _apiInfo = apiInfo;
            return this;
        }

        public SwaggerSpecConfig Authorization(string name, Authorization authorization)
        {
            _authorizations = _authorizations ?? new Dictionary<string, Authorization>();
            _authorizations[name] = authorization;
            return this;
        }

        public SwaggerGenerator GetGenerator(HttpRequestMessage swaggerRequest)
        {
            var basePath = BasePathResolver(swaggerRequest);
            var targetVersion = _targetVersionResolver(swaggerRequest);

            var apiDescriptions = swaggerRequest.GetConfiguration().Services.GetApiExplorer()
                .ApiDescriptions
                .Where(apiDesc => !_ignoreObsoleteActions || apiDesc.IsNotObsolete())
                .Where(apiDesc => !_shouldIgnoreResolver(apiDesc))
                .Where(apiDesc => _versionSupportResolver(apiDesc, targetVersion));

            var options = new SwaggerGeneratorOptions(
                _declarationNameResolver,
                _resourceNameComparer,
                _customTypeMappings,
                _polymorphicTypes,
                _modelFilterFactories.Select(factory => factory()),
                _operationFilterFactories.Select(factory => factory()),
                _apiInfo,
                _authorizations
                );

            return new SwaggerGenerator(
                basePath,
                targetVersion,
                apiDescriptions,
                options);
        }
    }
}