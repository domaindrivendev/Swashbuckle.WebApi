using System;
using System.Linq;
using System.Reflection;
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

            ApplyParamComments(operation, methodNode);

            ApplyResponseComments(operation, schemaRegistry, methodNode);
        }

        private static void ApplyParamComments(Operation operation, XPathNavigator methodNode)
        {
            if (operation.parameters == null) return;

            var paramNodes = methodNode.Select(ParameterTag);
            while (paramNodes.MoveNext())
            {
                var paramNode = paramNodes.Current;
                var parameter = operation.parameters.SingleOrDefault(param => param.name == paramNode.GetAttribute("name", ""));
                if (parameter != null)
                    parameter.description = paramNode.ExtractContent();
            }
        }

        private static void ApplyResponseComments(Operation operation, SchemaRegistry schemaRegistry, XPathNavigator methodNode)
        {
            var responseNodes = methodNode.Select(ResponseTag);

            if (responseNodes.Count <= 0) return;

            var successResponse = operation.responses.First().Value;
            operation.responses.Clear();

            while (responseNodes.MoveNext())
            {
                var statusCode = responseNodes.Current.GetAttribute("code", string.Empty);
                var description = responseNodes.Current.ExtractContent();

                var responseTypeName = responseNodes.Current.GetAttribute("cref", string.Empty);
                var schema = responseTypeName == string.Empty
                    ? (statusCode.StartsWith("2") 
                        ? successResponse.schema 
                        : null
                    )
                    : GetCrefSchema(schemaRegistry, responseTypeName.Substring(2));
                var response = new Response
                {
                    description = description,
                    schema = schema
                };
                operation.responses[statusCode] = response;
            }
        }

        private static Schema GetCrefSchema(SchemaRegistry schemaRegistry, string responseTypeName)
        {
            Schema schema ;
            if (schemaRegistry.Definitions.TryGetValue(responseTypeName, out schema))
            {
                return schema;
            }
            
            var responseType = GetType(responseTypeName);
            if (responseType == null)
                throw new Exception($"Could not resolve type {responseTypeName}");
            schema = schemaRegistry.GetOrRegister(responseType);
            return schema;
        }

        private static Assembly[] _assemblies = AppDomain.CurrentDomain.GetAssemblies();
        private static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var assembly in _assemblies)
            {
                type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }
    }
}