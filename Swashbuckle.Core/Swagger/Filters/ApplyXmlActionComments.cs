using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Xml.XPath;
using Swashbuckle.Swagger;

namespace Swashbuckle.Swagger.Filters
{
    public class ApplyXmlActionComments : IOperationFilter
    {
        private const string MethodExpression = "/doc/members/member[@name='M:{0}.{1}{2}']";
        private const string SummaryExpression = "summary";
        private const string RemarksExpression = "remarks";
        private const string ParameterExpression = "param";
        private const string ResponseExpression = "response";

        private readonly XPathNavigator _navigator;

        public ApplyXmlActionComments(string xmlCommentsPath)
        {
            _navigator = new XPathDocument(xmlCommentsPath).CreateNavigator();
        }

        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var methodNode = _navigator.SelectSingleNode(XPathFor(apiDescription.ActionDescriptor));
            if (methodNode == null) return;

            var summaryNode = methodNode.SelectSingleNode(SummaryExpression);
            if (summaryNode != null)
                operation.summary = summaryNode.Value.Trim();

            var remarksNode = methodNode.SelectSingleNode(RemarksExpression);
            if (remarksNode != null)
                operation.description = remarksNode.Value.Trim();

            ApplyParamComments(operation, methodNode);

            // TODO: Not sure about this feature???
            //ApplyResponseComments(operation, methodNode);
        }

        private static string XPathFor(HttpActionDescriptor actionDescriptor)
        {
            var controllerName = actionDescriptor.ControllerDescriptor.ControllerType.FullName;
            var reflectedActionDescriptor = actionDescriptor as ReflectedHttpActionDescriptor;
            var actionName = (reflectedActionDescriptor != null)
                ? reflectedActionDescriptor.MethodInfo.Name
                : actionDescriptor.ActionName;

            var paramTypeNames = actionDescriptor.GetParameters()
                .Select(paramDesc => TypeNameFor(paramDesc.ParameterType))
                .ToArray();

            var parameters = (paramTypeNames.Any())
                ? String.Format("({0})", String.Join(",", paramTypeNames))
                : String.Empty;

            return String.Format(MethodExpression, controllerName, actionName, parameters);
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

        private static void ApplyParamComments(Operation operation, XPathNavigator methodNode)
        {
            var paramNodes = methodNode.Select(ParameterExpression);
            while (paramNodes.MoveNext())
            {
                var paramNode = paramNodes.Current;
                var parameter = operation.parameters.SingleOrDefault(param => param.name == paramNode.GetAttribute("name", ""));
                if (parameter != null)
                    parameter.description = paramNode.Value.Trim();
            }
        }

        private static void ApplyResponseComments(Operation operation, XPathNavigator methodNode)
        {
            var responseNodes = methodNode.Select(ResponseExpression);
            while (responseNodes.MoveNext())
            {
                var responseNode = responseNodes.Current;
                var response = new Response { description = responseNode.Value.Trim() };
                operation.responses[responseNode.GetAttribute("code", "")] = response;
            }
        }
    }
}