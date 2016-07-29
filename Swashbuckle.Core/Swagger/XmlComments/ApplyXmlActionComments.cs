using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Xml.XPath;
using Swashbuckle.Swagger;

namespace Swashbuckle.Swagger.XmlComments
{
    public class ApplyXmlActionComments : IOperationFilter
    {
        private const string MemberXPath = "/doc/members/member[@name='{0}']";
        private const string SummaryTag = "summary";
        private const string RemarksTag = "remarks";
        private const string ParameterTag = "param";
        private const string ResponseTag = "response";

        private readonly XPathNavigator _navigator;

        public ApplyXmlActionComments(string xmlCommentsPath)
        {
            _navigator = new XPathDocument(xmlCommentsPath).CreateNavigator();
        }

        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var reflectedActionDescriptor = apiDescription.ActionDescriptor as ReflectedHttpActionDescriptor;
            if (reflectedActionDescriptor == null) return;

            var commentId = XmlCommentsIdHelper.GetCommentIdForMethod(reflectedActionDescriptor.MethodInfo);
            var methodNode = _navigator.SelectSingleNode(string.Format(MemberXPath, commentId));
            if (methodNode == null) return;

            var summaryNode = methodNode.SelectSingleNode(SummaryTag);
            if (summaryNode != null)
                operation.summary = summaryNode.ExtractContent();

            var remarksNode = methodNode.SelectSingleNode(RemarksTag);
            if (remarksNode != null)
                operation.description = remarksNode.ExtractContent();

            ApplyParamComments(operation, methodNode, reflectedActionDescriptor.MethodInfo);

            ApplyResponseComments(operation, methodNode);
        }

        private static void ApplyParamComments(Operation operation, XPathNavigator methodNode, MethodInfo method)
        {
            if (operation.parameters == null) return;

            var parameterNames = (from param in method.GetParameters()
                let attribute =
                    param.GetCustomAttributes(typeof (FromUriAttribute), true)
                        .Cast<FromUriAttribute>()
                        .FirstOrDefault()
                select new {uriName = !string.IsNullOrWhiteSpace(attribute?.Name) ? attribute.Name : param.Name, name = param.Name}).ToDictionary(x=>x.uriName, z=>z.name);

                             var paramNodes = methodNode.Select(ParameterTag);
            while (paramNodes.MoveNext())
            {
                var paramNode = paramNodes.Current;
                var parameter = operation.parameters.SingleOrDefault(param =>
                {
                    string name = null;
                    if (!parameterNames.TryGetValue(param.name, out name))
                    {
                        name = param.name;
                    }
                    return name.Equals(paramNode.GetAttribute("name", ""), StringComparison.OrdinalIgnoreCase);
                });
                if (parameter != null)
                    parameter.description = paramNode.ExtractContent();
            }
        }

        private static void ApplyResponseComments(Operation operation, XPathNavigator methodNode)
        {
            var responseNodes = methodNode.Select(ResponseTag);

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