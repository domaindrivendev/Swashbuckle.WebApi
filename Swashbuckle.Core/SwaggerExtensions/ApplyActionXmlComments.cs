
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Xml.XPath;
using Swashbuckle.Swagger;

namespace Swashbuckle.SwaggerExtensions
{
    public class ApplyActionXmlComments : IOperationFilter
    {
        private const string MethodExpression = "/doc/members/member[@name='M:{0}.{1}{2}']";
        private const string SummaryExpression = "summary";
        private const string RemarksExpression = "remarks";
		private const string ParameterExpression = "param[@name=\"{0}\"]";
		private const string ResponseExpression = "response";
		private const string TypeNameAttributeExpression = "cref";
		private const string ReturnsExpression = "returns[@" + TypeNameAttributeExpression + "]";

        private readonly XPathNavigator _navigator;

        public ApplyActionXmlComments(string xmlCommentsPath)
        {
            _navigator = new XPathDocument(xmlCommentsPath).CreateNavigator();
        }

        public void Apply(Operation operation, DataTypeRegistry dataTypeRegistry, ApiDescription apiDescription)
        {
            var methodNode = _navigator.SelectSingleNode(GetXPathFor(apiDescription.ActionDescriptor));

            operation.Summary = GetChildValueOrDefault(methodNode, SummaryExpression);
            operation.Notes = GetChildValueOrDefault(methodNode, RemarksExpression);

            foreach (var paramDesc in apiDescription.ParameterDescriptions)
            {
                if (paramDesc.ParameterDescriptor == null) continue; // not in action signature (e.g. route parameter)

                var parameter = operation.Parameters.SingleOrDefault(p => p.Name == paramDesc.Name);
                if (parameter == null) continue;

                parameter.Description = GetChildValueOrDefault(
                    methodNode,
                    String.Format(ParameterExpression, paramDesc.ParameterDescriptor.ParameterName));
            }

            if (methodNode == null) return;

            foreach (var responseMessage in GetResponseMessages(methodNode))
            {
                operation.ResponseMessages.Add(responseMessage);
            }

			var returnType = GetCustomReturnType(methodNode);
			if (returnType == null) return;

			var dataType = dataTypeRegistry.GetOrRegister(returnType);
			operation.Type = dataType.Id;
        }

		private static Type GetCustomReturnType(XPathNavigator methodNode)
		{
			var attributeValue = GetAttributeValueOrDefault(methodNode, ReturnsExpression, TypeNameAttributeExpression);
			if (attributeValue == null || !attributeValue.StartsWith("T:") || attributeValue.Length < 3) return null;
			var returnTypeName = attributeValue.Substring(2);

			var type = AppDomain
				.CurrentDomain
				.GetAssemblies()
				.Where(a => a != null)
				.SelectMany(a => a.GetTypes())
				.Where(t => t != null)
				.FirstOrDefault(t => t.FullName.Equals(returnTypeName));

			return type;
		}

        private static string GetXPathFor(HttpActionDescriptor actionDescriptor)
        {
            var controllerName = actionDescriptor.ControllerDescriptor.ControllerType.FullName;
            var actionName = actionDescriptor.ActionName;

            if (actionDescriptor.GetCustomAttributes<ActionNameAttribute>().Any())
            {
                var reflected = actionDescriptor as ReflectedHttpActionDescriptor;
                if (reflected != null)
                {
                    actionName = reflected.MethodInfo.Name;
                }
            }

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

		private static string GetChildValueOrDefault(XPathNavigator node, string childExpression)
		{
			if (node == null) return String.Empty;

			var childNode = node.SelectSingleNode(childExpression);
			return (childNode == null) ? String.Empty : childNode.Value.Trim();
		}        

		private static string GetAttributeValueOrDefault(XPathNavigator node, string childExpression, string attributeName)
		{
			if (node == null) return null;

			var childNode = node.SelectSingleNode(childExpression);
			if (childNode == null) return null;

			var attribute = childNode.GetAttribute(attributeName, string.Empty);
			return (attribute == null) ? null : attribute.Trim();
		}

        private static IEnumerable<ResponseMessage> GetResponseMessages(XPathNavigator node)
        {
            var iterator = node.Select(ResponseExpression);
            while (iterator.MoveNext())
            {
                yield return new ResponseMessage
                {
                    Code = Int32.Parse(iterator.Current.GetAttribute("code", String.Empty)),
                    Message = iterator.Current.Value
                };
            }
        }
    }
}