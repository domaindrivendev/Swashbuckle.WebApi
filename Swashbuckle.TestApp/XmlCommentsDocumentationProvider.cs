using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Swashbuckle.TestApp
{
    /// <summary>
    /// Accesses the XML doc blocks written in code to further document the API.
    /// A modified version taken from Swagger.NET
    /// </summary>
    public class XmlCommentsDocumentationProvider : IDocumentationProvider
    {
        readonly XPathNavigator _documentNavigator;
        private const string MethodExpression = "/doc/members/member[@name='M:{0}']";
        private static readonly Regex NullableTypeNameRegex = new Regex(@"(.*\.Nullable)" + Regex.Escape("`1[[") + "([^,]*),.*");

        public XmlCommentsDocumentationProvider(string documentPath)
        {
            var xpath = new XPathDocument(documentPath);
            _documentNavigator = xpath.CreateNavigator();
        }

        public virtual string GetDocumentation(HttpParameterDescriptor parameterDescriptor)
        {
            var reflectedParameterDescriptor = parameterDescriptor as ReflectedHttpParameterDescriptor;
            if (reflectedParameterDescriptor != null)
            {
                var memberNode = GetMemberNode(reflectedParameterDescriptor.ActionDescriptor);
                if (memberNode != null)
                {
                    var parameterName = reflectedParameterDescriptor.ParameterInfo.Name;
                    var parameterNode = memberNode.SelectSingleNode(string.Format("param[@name='{0}']", parameterName));
                    if (parameterNode != null)
                    {
                        return parameterNode.Value.Trim();
                    }
                }
            }

            return "No Documentation Found.";
        }

        public virtual string GetDocumentation(HttpActionDescriptor actionDescriptor)
        {
            var result = new XElement("documentation", "No Documentation Found.");
            var memberNode = GetMemberNode(actionDescriptor);
            if (memberNode != null)
            {
                var summaryNode = memberNode.SelectSingleNode("summary");
                if (summaryNode != null)
                {
                    result.Add(new XElement(summaryNode.Name, summaryNode.Value), GetNotes(actionDescriptor));
                    result.Add(GetResponses(actionDescriptor).ToArray());
                }
            }

            return result.ToString();
        }

        private XElement GetNotes(HttpActionDescriptor actionDescriptor)
        {
            var memberNode = GetMemberNode(actionDescriptor);
            if (memberNode != null)
            {
                var summaryNode = memberNode.SelectSingleNode("remarks");
                if (summaryNode != null)
                {
                    return new XElement(summaryNode.Name, summaryNode.Value);
                }
            }

            return null;
        }

        private List<XElement> GetResponses(HttpActionDescriptor actionDescriptor)
        {
            var memberNode = GetMemberNode(actionDescriptor);
            return (from XPathNavigator node in memberNode.Select("response") select new XElement(node.Name, node.Value, new XAttribute("code", node.GetAttribute("code", "")))).ToList();
        }

        private XPathNavigator GetMemberNode(HttpActionDescriptor actionDescriptor)
        {
            var reflectedActionDescriptor = actionDescriptor as ReflectedHttpActionDescriptor;
            if (reflectedActionDescriptor != null)
            {
                var selectExpression = string.Format(MethodExpression, GetMemberName(reflectedActionDescriptor.MethodInfo));
                var node = _documentNavigator.SelectSingleNode(selectExpression);
                if (node != null)
                {
                    return node;
                }
            }

            return null;
        }

        private static string GetMemberName(MethodBase method)
        {
            if (method.DeclaringType != null)
            {
                var name = string.Format("{0}.{1}", method.DeclaringType.FullName, method.Name);
                var parameters = method.GetParameters();
                if (parameters.Length != 0)
                {
                    var parameterTypeNames = parameters.Select(param => ProcessTypeName(param.ParameterType.FullName)).ToArray();
                    name += string.Format("({0})", string.Join(",", parameterTypeNames));
                }

                return name;
            }
            return null;
        }

        private static string ProcessTypeName(string typeName)
        {
            //handle nullable
            var result = NullableTypeNameRegex.Match(typeName);
            return result.Success ? string.Format("{0}{{{1}}}", result.Groups[1].Value, result.Groups[2].Value) : typeName;
        }
    }
}
