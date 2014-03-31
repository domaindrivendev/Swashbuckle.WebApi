using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Description;
using System.Xml.XPath;
using Swashbuckle.Core.Swagger;

namespace Swashbuckle.Core.Application
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
            VersionResolver = (req) => "1.0";
            BasePathResolver = DefaultBasePathResolver;
            DeclarationKeySelector = (apiDesc) => apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName;
            CustomTypeMappings = new Dictionary<Type, ModelSpec>();
            SubTypesLookup = new Dictionary<Type, IEnumerable<Type>>();
            OperationSpecFilters = new List<IOperationSpecFilter>();
            IgnoreObsoleteActionsFlag = false;
        }

        internal Func<HttpRequestMessage, string> VersionResolver { get; private set; }
        internal Func<HttpRequestMessage, string> BasePathResolver { get; private set; }
        internal Func<ApiDescription, string> DeclarationKeySelector { get; private set; }
        internal Dictionary<Type, ModelSpec> CustomTypeMappings { get; private set; }
        internal Dictionary<Type, IEnumerable<Type>> SubTypesLookup = new Dictionary<Type, IEnumerable<Type>>();
        internal List<IOperationSpecFilter> OperationSpecFilters = new List<IOperationSpecFilter>();
        internal bool IgnoreObsoleteActionsFlag { get; private set; }

        public SwaggerSpecConfig ResolveApiVersion(Func<HttpRequestMessage, string> versionResolver)
        {
            if (versionResolver == null) throw new ArgumentNullException("versionResolver");
            VersionResolver = versionResolver;
            return this;
        }

        public SwaggerSpecConfig ResolveBasePath(Func<HttpRequestMessage, string> basePathResolver)
        {
            if (basePathResolver == null) throw new ArgumentNullException("basePathResolver");
            BasePathResolver = basePathResolver;
            return this;
        }

        public SwaggerSpecConfig GroupDeclarationsBy(Func<ApiDescription, string> declarationKeySelector)
        {
            if (declarationKeySelector == null) throw new ArgumentNullException("declarationKeySelector");
            DeclarationKeySelector = declarationKeySelector;
            return this;
        }

        public SwaggerSpecConfig IgnoreObsoleteActions()
        {
            IgnoreObsoleteActionsFlag = true;
            return this;
        }

        public SwaggerSpecConfig MapType<T>(ModelSpec modelSpec)
        {
            CustomTypeMappings[typeof (T)] = modelSpec;
            return this;
        }

        public SubTypeList<TBase> SubTypesOf<TBase>(params Type[] subTypes)
        {
            var baseType = typeof(TBase);
            IEnumerable<Type> subTypeList;

            if (!SubTypesLookup.TryGetValue(baseType, out subTypeList))
            {
                subTypeList = new SubTypeList<TBase>();
                SubTypesLookup.Add(baseType, subTypeList);
            }

            return (SubTypeList<TBase>)subTypeList;
        }

        public SwaggerSpecConfig OperationSpecFilter<T>()
            where T : IOperationSpecFilter, new()
        {
            return OperationSpecFilter(new T());
        }

        public SwaggerSpecConfig OperationSpecFilter(IOperationSpecFilter operationSpecFilter)
        {
            if (operationSpecFilter == null) throw new ArgumentNullException("operationSpecFilter");
            OperationSpecFilters.Add(operationSpecFilter);
            return this;
        }

        public SwaggerSpecConfig IncludeXmlComments(string xmlCommentsPath)
        {
            var xmlCommentsDoc = new XPathDocument(xmlCommentsPath);
            OperationSpecFilters.Add(new ApplyActionXmlComments(xmlCommentsDoc));
            return this;
        }

        private static string DefaultBasePathResolver(HttpRequestMessage request)
        {
            var requestUri = request.RequestUri;
            return requestUri.GetLeftPart(UriPartial.Authority) + request.GetConfiguration().VirtualPathRoot;
        }
    }

    public class SubTypeList<TBase> : IEnumerable<Type>
    {
        readonly List<Type> _subTypes = new List<Type>();

        public SubTypeList<TBase> Include<T>()
            where T : TBase
        {
            var type = typeof(T);
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
}