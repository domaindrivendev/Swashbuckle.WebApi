using System;
using System.Xml.XPath;

namespace Swashbuckle.Core.Swagger
{
    public class ApplyTypeXmlComments : IModelFilter
    {
        private const string TypeExpression = "/doc/members/member[@name='T:{0}']";
        private const string PropertyExpression = "/doc/members/member[@name='P:{0}.{1}']";
        private const string SummaryExpression = "summary";

        private readonly XPathNavigator _navigator;

        public ApplyTypeXmlComments(XPathDocument xmlCommentsDoc)
        {
            _navigator = xmlCommentsDoc.CreateNavigator();
        }

        public void Apply(DataType model, DataTypeRegistry dataTypeRegistry, Type type)
        {
            var typeNode = _navigator.SelectSingleNode(String.Format(TypeExpression, type.FullName));
            model.Description = GetChildValueOrDefault(typeNode, SummaryExpression);

            foreach (var property in model.Properties)
            {
                var propertyNode = _navigator.SelectSingleNode(String.Format(PropertyExpression, type.FullName, property.Key));
                property.Value.Description = GetChildValueOrDefault(propertyNode, SummaryExpression);
            }
        }

        private static string GetChildValueOrDefault(XPathNavigator node, string childExpression)
        {
            if (node == null) return null;

            var childNode = node.SelectSingleNode(childExpression);
            return (childNode == null) ? null : childNode.Value.Trim();
        }
    }
}