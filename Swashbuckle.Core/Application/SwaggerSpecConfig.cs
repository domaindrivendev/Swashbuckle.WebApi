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
        private Func<ApiDescription, string, bool> _versionSupportResolver; // obsolete
        private Func<ApiDescription, IEnumerable<string>> _applicableVersionsResolver;
        private bool _ignoreObsoleteActions;
        private Func<ApiDescription, string> _resourceNameResolver;
        private IComparer<string> _groupComparer;
        private readonly Dictionary<Type, Func<DataType>> _customTypeMappings;
        private readonly List<PolymorphicType> _polymorphicTypes;
        private readonly List<Func<IModelFilter>> _modelFilterFactories;
        private readonly List<Func<IOperationFilter>> _operationFilterFactories;

        public SwaggerSpecConfig()
        {
            BasePathResolver = (req) => req.RequestUri.GetLeftPart(UriPartial.Authority) + req.GetConfiguration().VirtualPathRoot.TrimEnd('/');
            _targetVersionResolver = (req) => "1.0"; // obsolete
            _versionSupportResolver = (apiDesc, version) => true; // obsolete
            _applicableVersionsResolver = (apiDesc) => new[] { "*" };
            _ignoreObsoleteActions = false;
            _resourceNameResolver = (apiDesc) => apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName;
            _groupComparer = Comparer<string>.Default;
            _customTypeMappings = new Dictionary<Type, Func<DataType>>();
            _polymorphicTypes = new List<PolymorphicType>();
            _modelFilterFactories = new List<Func<IModelFilter>>();
            _operationFilterFactories = new List<Func<IOperationFilter>>();
        }

        internal Func<HttpRequestMessage, string> BasePathResolver { get; private set; }

        public SwaggerSpecConfig ResolveBasePathUsing(Func<HttpRequestMessage, string> basePathResolver)
        {
            if (basePathResolver == null)
                throw new ArgumentNullException("basePathResolver");
            BasePathResolver = basePathResolver;
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
            _targetVersionResolver = (req) => apiVersion;
            _applicableVersionsResolver = (apiDesc) => new[] { apiVersion };
            return this;
        }

        public SwaggerSpecConfig SupportMultipleApiVersions(Func<ApiDescription, IEnumerable<string>> applicableVersionsResolver)
        {
            _targetVersionResolver = (req) => req.GetRouteData().Values["apiVersion"].ToString();

            if (applicableVersionsResolver == null)
                throw new ArgumentNullException("applicableVersionsResolver");
            _applicableVersionsResolver = applicableVersionsResolver;
            return this;
        }

        public SwaggerSpecConfig IgnoreObsoleteActions()
        {
            _ignoreObsoleteActions = true;
            return this;
        }

        public SwaggerSpecConfig GroupDeclarationsBy(Func<ApiDescription, string> resourceNameResolver)
        {
            if (resourceNameResolver == null)
                throw new ArgumentNullException("resourceNameResolver");
            _resourceNameResolver = resourceNameResolver;
            return this;
        }

        public SwaggerSpecConfig SortDeclarationsBy(IComparer<string> groupComparer)
        {
            if (groupComparer == null) throw new ArgumentNullException("groupComparer");
            _groupComparer = groupComparer;
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

        public IEnumerable<string> GetDiscoveryUrls(HttpRequestMessage swaggerRequest)
        {
            var basePath = BasePathResolver(swaggerRequest);

            var apiVersions = swaggerRequest.GetConfiguration().Services.GetApiExplorer()
                .ApiDescriptions
                .SelectMany(apiDesc => _applicableVersionsResolver(apiDesc))
                .Distinct()
                .OrderBy(v => v);

            if (apiVersions.Contains("*"))
                return new[] { String.Format("{0}/swagger/api-docs", basePath) };

            return apiVersions
                .Select(apiVersion => String.Format("{0}/swagger/{1}/api-docs", basePath, apiVersion));
        }

        public SwaggerGenerator GetGenerator(HttpRequestMessage swaggerRequest)
        {
            var basePath = BasePathResolver(swaggerRequest);
            var targetVersion = _targetVersionResolver(swaggerRequest);

            var apiDescriptions = GetApplicableApiDescriptions(
                swaggerRequest.GetConfiguration().Services.GetApiExplorer().ApiDescriptions,
                targetVersion);

            var options = new SwaggerGeneratorOptions(
                _resourceNameResolver,
                _groupComparer,
                _customTypeMappings,
                _polymorphicTypes,
                _modelFilterFactories.Select(factory => factory()),
                _operationFilterFactories.Select(factory => factory())
                );

            return new SwaggerGenerator(
                basePath,
                targetVersion,
                apiDescriptions,
                options);
        }

        private IEnumerable<ApiDescription> GetApplicableApiDescriptions(
            IEnumerable<ApiDescription> apiDescriptions,
            string targetVersion)
        {
            return apiDescriptions
                .Where(apiDesc => _versionSupportResolver(apiDesc, targetVersion)) // obsolete
                .Where(apiDesc =>
                {
                    var applicableVersions = _applicableVersionsResolver(apiDesc);
                    return applicableVersions.Contains("*") || applicableVersions.Contains(targetVersion);
                })
                .Where(apiDesc => !_ignoreObsoleteActions || !apiDesc.IsMarkedObsolete());
        }
    }
}