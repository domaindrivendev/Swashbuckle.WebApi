using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Xml.XPath;
using Swashbuckle.Swagger;

namespace Swashbuckle.Swagger.XmlComments
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
                operation.summary = summaryNode.ExtractContent();

            var remarksNode = methodNode.SelectSingleNode(RemarksExpression);
            if (remarksNode != null)
                operation.description = remarksNode.ExtractContent();

            ApplyParamComments(operation, methodNode);

            ApplyResponseComments(operation, methodNode);
        }

		private static string XPathFor(HttpActionDescriptor actionDescriptor)
        {
            var controllerName = actionDescriptor.ControllerDescriptor.ControllerType.FullName;
            var reflectedActionDescriptor = actionDescriptor as ReflectedHttpActionDescriptor;
            var actionName = (reflectedActionDescriptor != null)
                ? reflectedActionDescriptor.MethodInfo.Name
                : actionDescriptor.ActionName;

            var paramTypeNames = actionDescriptor.GetParameters()
                .Select(paramDesc => paramDesc.ParameterType.XmlCommentsQualifier())
                .ToArray();

            var parameters = (paramTypeNames.Any())
                ? String.Format("({0})", String.Join(",", paramTypeNames))
                : String.Empty;

            return String.Format(MethodExpression, controllerName, actionName, parameters);
        }

        private static void ApplyParamComments(Operation operation, XPathNavigator methodNode)
        {
            var paramNodes = methodNode.Select(ParameterExpression);
            while (paramNodes.MoveNext())
            {
                var paramNode = paramNodes.Current;
                var parameter = operation.parameters.SingleOrDefault(param => param.name == paramNode.GetAttribute("name", ""));
                if (parameter != null)
                    parameter.description = paramNode.ExtractContent();
            }
        }

        private static void ApplyResponseComments(Operation operation, XPathNavigator methodNode)
        {
            var responseNodes = methodNode.Select(ResponseExpression);

            if (responseNodes.Count > 0)
            {
                var successResponse = operation.responses.First().Value;
                operation.responses.Clear();

                while (responseNodes.MoveNext())
                {
                    var statusCode = responseNodes.Current.GetAttribute("code", "");
                    var description = responseNodes.Current.ExtractContent();

                    var response = new Response
                    {
                        description = description,
                        schema = statusCode.StartsWith("2") ? successResponse.schema : null
                    };
                    operation.responses[statusCode] = response;
                }
            }
        }
    }
}