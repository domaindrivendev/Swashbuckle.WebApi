using System;
using System.Xml.XPath;
using Swashbuckle.Swagger;

namespace Swashbuckle.SwaggerExtensions
{
    public class ApplyXmlTypeComments : ISchemaFilter
    {
        private const string TypeExpression = "/doc/members/member[@name='T:{0}']";
        private const string SummaryExpression = "summary";
        private const string PropertyExpression = "/doc/members/member[@name='P:{0}.{1}']";

        private readonly XPathNavigator _navigator;

        public ApplyXmlTypeComments(string xmlCommentsPath)
        {
            _navigator = new XPathDocument(xmlCommentsPath).CreateNavigator();
        }

        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            var typeNode = _navigator.SelectSingleNode(String.Format(TypeExpression, type.FullName));
            if (typeNode == null) return;

            var summaryNode = typeNode.SelectSingleNode(SummaryExpression);
            if (summaryNode != null)
                schema.description = summaryNode.Value.Trim();

            foreach (var property in schema.properties)
            {
                var propertyNode = _navigator.SelectSingleNode(String.Format(PropertyExpression, type.FullName, property.Key));
                if (propertyNode != null)
                    property.Value.description = propertyNode.Value.Trim();
            }
        }
    }
}