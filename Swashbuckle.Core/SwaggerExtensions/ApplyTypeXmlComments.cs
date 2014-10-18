using System;
using System.Xml.XPath;
using Swashbuckle.Swagger;

namespace Swashbuckle.SwaggerExtensions
{
    public class ApplyTypeXmlComments : IModelFilter
    {
        private const string TypeExpression = "/doc/members/member[@name='T:{0}']";
        private const string PropertyExpression = "/doc/members/member[@name='P:{0}.{1}']";
        private const string SummaryExpression = "summary";

        private readonly XPathNavigator _navigator;

        public ApplyTypeXmlComments(string xmlCommentsPath)
        {
            _navigator = new XPathDocument(xmlCommentsPath).CreateNavigator();
        }

        public void Apply(DataType model, DataTypeRegistry dataTypeRegistry, Type type)
        {
            var typeNode = _navigator.SelectSingleNode(String.Format(TypeExpression, type.FullName));
            if (typeNode == null) return;

            var summary = GetChildValue(typeNode, SummaryExpression);
            if (summary != null) model.Description = summary; 

            foreach (var property in model.Properties)
            {
                var propertyNode = _navigator.SelectSingleNode(String.Format(PropertyExpression, type.FullName, property.Key));
                if (propertyNode == null) continue;

                summary = GetChildValue(propertyNode, SummaryExpression);
                if (summary != null) property.Value.Description = summary;
            }
        }

        private static string GetChildValue(XPathNavigator node, string childExpression)
        {
            var childNode = node.SelectSingleNode(childExpression);
            return (childNode == null) ? null : childNode.Value.Trim();
        }
    }
}