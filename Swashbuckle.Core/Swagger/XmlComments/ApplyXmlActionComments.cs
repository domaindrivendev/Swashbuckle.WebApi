using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Xml.XPath;

namespace Swashbuckle.Swagger.XmlComments
{
    public class ApplyXmlActionComments : IOperationFilter
    {
        private const string MemberXPath = "/doc/members/member[@name='{0}']";
        private const string SummaryXPath = "summary";
        private const string RemarksXPath = "remarks";
        private const string ParamXPath = "param[@name='{0}']";
        private const string ResponseXPath = "response";

        private readonly XPathDocument _xmlDoc;

        public ApplyXmlActionComments(string filePath)
            : this(new XPathDocument(filePath)) { }

        public ApplyXmlActionComments(XPathDocument xmlDoc)
        {
            _xmlDoc = xmlDoc;
        }

        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var reflectedActionDescriptor = apiDescription.ActionDescriptor as ReflectedHttpActionDescriptor;
            if (reflectedActionDescriptor == null) return;

            XPathNavigator navigator;
            lock (_xmlDoc)
            {
                navigator = _xmlDoc.CreateNavigator();
            }

            var commentId = XmlCommentsIdHelper.GetCommentIdForMethod(reflectedActionDescriptor.MethodInfo);
            var methodNode = navigator.SelectSingleNode(string.Format(MemberXPath, commentId));
            if (methodNode == null) return;

            var summaryNode = methodNode.SelectSingleNode(SummaryXPath);
            if (summaryNode != null)
                operation.summary = summaryNode.ExtractContent();

            var remarksNode = methodNode.SelectSingleNode(RemarksXPath);
            if (remarksNode != null)
                operation.description = remarksNode.ExtractContent();

            ApplyParamComments(operation, methodNode, reflectedActionDescriptor.MethodInfo);

            ApplyResponseComments(operation, methodNode);
        }

        private static void ApplyParamComments(Operation operation, XPathNavigator methodNode, MethodInfo method)
        {
            if (operation.parameters == null) return;

            foreach (var parameter in operation.parameters)
            {
                // Inspect method to find the corresponding action parameter
                // NOTE: If a parameter binding is present (e.g. [FromUri(Name..)]), then the lookup needs
                // to be against the "bound" name and not the actual parameter name
                var actionParameter = method.GetParameters()
                    .FirstOrDefault(paramInfo =>
                        HasBoundName(paramInfo, parameter.name) || paramInfo.Name == parameter.name
                     );
                if (actionParameter == null) continue;

                var paramNode = methodNode.SelectSingleNode(string.Format(ParamXPath, actionParameter.Name));
                if (paramNode != null)
                    parameter.description = paramNode.ExtractContent();
            }
        }

        private static void ApplyResponseComments(Operation operation, XPathNavigator methodNode)
        {
            var responseNodes = methodNode.Select(ResponseXPath);

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

        private static bool HasBoundName(ParameterInfo paramInfo, string name)
        {
            var fromUriAttribute = paramInfo.GetCustomAttributes(false)
                .OfType<FromUriAttribute>()
                .FirstOrDefault();

            return (fromUriAttribute != null && fromUriAttribute.Name == name);
        }
    }
}