
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Xml.XPath;
using Swashbuckle.Swagger;
using System.Text.RegularExpressions;

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
		private readonly IEnumerable<Type> _types;

        public ApplyActionXmlComments(string xmlCommentsPath)
        {
            _navigator = new XPathDocument(xmlCommentsPath).CreateNavigator();
			_types = AppDomain
				.CurrentDomain
				.GetAssemblies()
				.Where(a => a != null)
				.SelectMany(a => a.GetTypes())
				.Where(t => t != null);
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

            foreach (var responseMessage in GetResponseMessages(methodNode, dataTypeRegistry))
            {
                operation.ResponseMessages.Add(responseMessage);
            }

			var returnsNode = methodNode.SelectSingleNode(ReturnsExpression);
			var dataTypeId = GetCrefModel(returnsNode, dataTypeRegistry);

			operation.Type = dataTypeId;
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

		private IEnumerable<ResponseMessage> GetResponseMessages(XPathNavigator node, DataTypeRegistry dataTypeRegistry)
        {
            var iterator = node.Select(ResponseExpression);
            while (iterator.MoveNext())
            {
                yield return new ResponseMessage
                {
                    Code = Int32.Parse(iterator.Current.GetAttribute("code", String.Empty)),
                    Message = iterator.Current.Value,
					ResponseModel = GetCrefModel(iterator.Current, dataTypeRegistry)
                };
            }
        }

		private string GetCrefModel(XPathNavigator node, DataTypeRegistry dataTypeRegistry)
		{
			var attributeValue = node.GetAttribute("cref", string.Empty);
			if (attributeValue == null || !attributeValue.StartsWith("T:") || attributeValue.Length < 3) return null;

			var returnTypeName = attributeValue.Substring(2);
			var type = _types.FirstOrDefault(t => t.FullName.Equals(returnTypeName));
			if(type == null) return null;
			var registered = dataTypeRegistry.GetOrRegister(type);

			return registered != null ? registered.Id : null;
		}
    }
}