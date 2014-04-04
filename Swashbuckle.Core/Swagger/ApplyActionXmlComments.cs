using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Xml.XPath;

namespace Swashbuckle.Core.Swagger
{
    public class ApplyActionXmlComments : IOperationFilter
    {
        private const string MethodExpression = "/doc/members/member[@name='M:{0}.{1}({2})']";
        private const string SummaryExpression = "summary";
        private const string RemarksExpression = "remarks";
        private const string ParameterExpression = "param[@name=\"{0}\"]";

        private readonly XPathNavigator _navigator;

        public ApplyActionXmlComments(XPathDocument xmlCommentsDoc)
        {
            _navigator = xmlCommentsDoc.CreateNavigator();
        }

        public void Apply(Operation operation, Dictionary<string, DataType> complexModels, DataTypeGenerator dataTypeGenerator, ApiDescription apiDescription)
        {
            var methodNode = GetNodeFor(apiDescription.ActionDescriptor);

            operation.Summary = GetChildValueOrDefault(methodNode, SummaryExpression);
            operation.Notes = GetChildValueOrDefault(methodNode, RemarksExpression);

            foreach (var paramDesc in apiDescription.ParameterDescriptions)
            {
                var parameter = operation.Parameters.SingleOrDefault(p => p.Name == paramDesc.Name);
                if (parameter == null) continue;

                parameter.Description = GetChildValueOrDefault(methodNode, String.Format(ParameterExpression, paramDesc.Name));
            }
        }

        private XPathNavigator GetNodeFor(HttpActionDescriptor actionDescriptor)
        {
            var controllerName = actionDescriptor.ControllerDescriptor.ControllerType.FullName;
            var actionName = actionDescriptor.ActionName;
            var parameters = String.Join(",", actionDescriptor.GetParameters()
                .Select(paramDesc => TypeNameFor(paramDesc.ParameterType)));

            var xpath = String.Format(MethodExpression, controllerName, actionName, parameters);
            return _navigator.SelectSingleNode(xpath);
        }

        private static string TypeNameFor(Type type)
        {
            if (type.IsGenericType)
            {
                var genericArguments = type.GetGenericArguments()
                    .Select(TypeNameFor)
                    .ToArray();

                var builder = new StringBuilder(type.Namespace + "." + type.Name);

                return builder
                    .Replace(String.Format("`{0}", genericArguments.Count()), String.Empty)
                    .Append(String.Format("{{{0}}}", String.Join(",", genericArguments).TrimEnd(',')))
                    .ToString();
            }

            return type.Namespace + "." + type.Name;
        }

        private static string GetChildValueOrDefault(XPathNavigator node, string childExpression)
        {
            if (node == null) return null;

            var childNode = node.SelectSingleNode(childExpression);
            return (childNode == null) ? null : childNode.Value.Trim();
        }
    }
}
