using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Xml.XPath;

namespace Swashbuckle.Core.Swagger
{
    public class ApplyActionXmlComments : IOperationSpecFilter
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

        public void Apply(OperationSpec operationSpec, Dictionary<string, ModelSpec> complexModels, ModelSpecGenerator modelSpecGenerator, ApiDescription apiDescription)
        {
            var methodNode = GetNodeFor(apiDescription.ActionDescriptor);

            operationSpec.Summary = GetChildValueOrDefault(methodNode, SummaryExpression);
            operationSpec.Notes = GetChildValueOrDefault(methodNode, RemarksExpression);

            foreach (var paramDesc in apiDescription.ParameterDescriptions)
            {
                var paramSpec = operationSpec.Parameters.SingleOrDefault(p => p.Name == paramDesc.Name);
                if (paramSpec == null) continue;

                paramSpec.Description = GetChildValueOrDefault(methodNode, String.Format(ParameterExpression, paramDesc.Name));
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
