using System;
using System.Xml.XPath;
using Swashbuckle.Swagger;
using System.Collections.Generic;

namespace Swashbuckle.Swagger.Filters
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

            List<Type> typeList = new List<Type>();
            typeList.Add(type);
            while (typeList[typeList.Count - 1].BaseType != null)
            {
                typeList.Add(typeList[typeList.Count - 1].BaseType);
            }

            foreach (var property in schema.properties)
            {
                XPathNavigator propertyNode = null;

                foreach (Type t in typeList)
                {
                    propertyNode = _navigator.SelectSingleNode(String.Format(PropertyExpression, t.FullName, property.Key));
                    if (propertyNode != null) { break; }
                }
                if (propertyNode != null)
                    property.Value.description = propertyNode.Value.Trim();
            }
        }
    }
}