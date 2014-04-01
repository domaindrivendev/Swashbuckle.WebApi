using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
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
            PolymorphicTypes = new List<PolymorphicType>();
            OperationSpecFilters = new List<IOperationSpecFilter>();
            IgnoreObsoleteActionsFlag = false;
        }

        internal Func<HttpRequestMessage, string> VersionResolver { get; private set; }
        internal Func<HttpRequestMessage, string> BasePathResolver { get; private set; }
        internal Func<ApiDescription, string> DeclarationKeySelector { get; private set; }
        internal Dictionary<Type, ModelSpec> CustomTypeMappings { get; private set; }
        internal List<PolymorphicType> PolymorphicTypes { get; private set; }
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

        public SwaggerSpecConfig PolymorphicType<TBase>(Action<PolymorphicType<TBase>> configure)
        {
            var subTypeInfo = new PolymorphicType<TBase>();
            configure(subTypeInfo);
            PolymorphicTypes.Add(subTypeInfo);
            return this;
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

        private string InferDescriminatorFrom<T>(Expression<Func<T, object>> expression)
        {
            MemberExpression memberExpression;

            var unaryExpression = expression.Body as UnaryExpression;
            if (unaryExpression != null)
                memberExpression = unaryExpression.Operand as MemberExpression;
            else
                memberExpression = expression.Body as MemberExpression;

            if (memberExpression == null)
                throw new ArgumentException(String.Format("Failed to infer descriminator from provided expression - {0}", expression));

            return memberExpression.Member.Name;
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