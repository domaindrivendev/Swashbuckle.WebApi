using System;
using System.Xml.XPath;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Swashbuckle.Swagger;

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

        public void Apply(Schema schema, SchemaRegistry schemaMap, Type type)
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

            foreach (var entry in schema.properties)
            {
                ApplyPropertyComments(entry.Key, entry.Value, typeList);
            }
        }

        private void ApplyPropertyComments(string propertyKey, Schema schema, IEnumerable<Type> typeList)
        {
            // TODO: There's a flaw in using an IOperationFilter here because there's no sure way to correlate
            // a schema property name to it's original member name (e.g. if using JsonProperty). Hence, the
            // optimistic assumption below.
            var assumedMemberName = Char.ToUpper(propertyKey[0]) + propertyKey.Substring(1);

            XPathNavigator propertyNode = null;
            foreach (Type t in typeList)
            {
                var propertyXPath = String.Format(PropertyExpression, t.FullName, assumedMemberName);
                propertyNode = _navigator.SelectSingleNode(propertyXPath);
                if (propertyNode != null) break;
            }
            if (propertyNode == null) return;

            var propSummaryNode = propertyNode.SelectSingleNode(SummaryExpression);
            if (propSummaryNode == null) return;

            schema.description = propSummaryNode.Value.Trim();
        }
    }
}