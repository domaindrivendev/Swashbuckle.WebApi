using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Web.Http.Description;
using System.Xml.XPath;
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

        public SwaggerSpecConfig()
        {
            ResolveBasePath = (req) => req.RequestUri.GetLeftPart(UriPartial.Authority) + req.GetConfiguration().VirtualPathRoot.TrimEnd('/');
            ResolveTargetVersion = (req) => "1.0";
            IgnoreObsoleteActionsFlag = false;
            ResolveVersionSupport = (apiDesc, version) => true;
            ResolveResourceName = (apiDesc) => apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName;
            OperationFilters = new List<IOperationFilter>();
            CustomTypeMappings = new Dictionary<Type, Func<DataType>>();
            PolymorphicTypes = new List<PolymorphicType>();
            ModelFilters = new List<IModelFilter>();
        }

        internal Func<HttpRequestMessage, string> ResolveBasePath { get; private set; }
        internal Func<HttpRequestMessage, string> ResolveTargetVersion { get; private set; }
        internal Func<ApiDescription, string, bool> ResolveVersionSupport { get; private set; } 
        internal Func<ApiDescription, string> ResolveResourceName { get; private set; }
        internal bool IgnoreObsoleteActionsFlag { get; private set; }
        internal List<IOperationFilter> OperationFilters = new List<IOperationFilter>();
        internal Dictionary<Type, Func<DataType>> CustomTypeMappings { get; private set; }
        internal List<PolymorphicType> PolymorphicTypes { get; private set; }
        internal List<IModelFilter> ModelFilters { get; private set; }

        public SwaggerSpecConfig ResolveBasePathUsing(Func<HttpRequestMessage, string> resolveBasePath)
        {
            if (resolveBasePath == null) throw new ArgumentNullException("resolveBasePath");
            ResolveBasePath = resolveBasePath;
            return this;
        }

        public SwaggerSpecConfig ResolveTargetVersionUsing(Func<HttpRequestMessage, string> resolveTargetVersion)
        {
            if (resolveTargetVersion == null) throw new ArgumentNullException("resolveTargetVersion");
            ResolveTargetVersion = resolveTargetVersion;
            return this;
        }

        public SwaggerSpecConfig IgnoreObsoleteActions()
        {
            IgnoreObsoleteActionsFlag = true;
            return this;
        }

        public SwaggerSpecConfig ResolveVersionSupportUsing(Func<ApiDescription, string, bool> resolveVersionSupport)
        {
            if (resolveVersionSupport == null) throw new ArgumentNullException("resolveVersionSupport");
            ResolveVersionSupport = resolveVersionSupport;
            return this;
        }

        public SwaggerSpecConfig GroupDeclarationsBy(Func<ApiDescription, string> resolveResourceName)
        {
            if (resolveResourceName == null) throw new ArgumentNullException("resolveResourceName");
            ResolveResourceName = resolveResourceName;
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
            OperationFilters.Add(operationFilter);
            return this;
        }

        public SwaggerSpecConfig MapType<T>(Func<DataType> factoryMethod)
        {
            CustomTypeMappings[typeof (T)] = factoryMethod;
            return this;
        }

        public SwaggerSpecConfig PolymorphicType<TBase>(Action<BasePolymorphicType<TBase>> configure)
        {
            var polymorphicType = new BasePolymorphicType<TBase>();
            configure(polymorphicType);
            PolymorphicTypes.Add(polymorphicType);
            return this;
        }

        public SwaggerSpecConfig IncludeXmlComments(string xmlCommentsPath)
        {
            var xmlCommentsDoc = new XPathDocument(xmlCommentsPath);
            OperationFilters.Add(new ApplyActionXmlComments(xmlCommentsDoc));
            ModelFilters.Add(new ApplyTypeXmlComments(xmlCommentsDoc));
            return this;
        }
    }

    public class PolymorphicType<T> : PolymorphicType
    {
        public PolymorphicType(bool isBase)
            : base(typeof(T), isBase)
        { }

        public PolymorphicType<T> SubType<TSub>(Action<PolymorphicType<TSub>> configure = null)
            where TSub : T
        {
            var subTypeInfo = new PolymorphicType<TSub>(false);
            RegisterSubType(subTypeInfo);

            if (configure != null) configure(subTypeInfo);

            return this;
        }
    }

    public class BasePolymorphicType<T> : PolymorphicType<T>
    {
        public BasePolymorphicType()
            : base(true)
        {
        }

        public PolymorphicType<T> DiscriminateBy(Expression<Func<T, object>> expression)
        {
            DiscriminateBy(InferDiscriminatorFrom(expression));
            return this;
        }

        private static string InferDiscriminatorFrom(Expression<Func<T, object>> expression)
        {
            MemberExpression memberExpression;

            var unaryExpression = expression.Body as UnaryExpression;
            if (unaryExpression != null)
                memberExpression = unaryExpression.Operand as MemberExpression;
            else
                memberExpression = expression.Body as MemberExpression;

            if (memberExpression == null)
                throw new ArgumentException(String.Format("Failed to infer discriminator from provided expression - {0}", expression));

            return memberExpression.Member.Name;
        }
    }
}