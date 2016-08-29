using System;
using System.Xml.XPath;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using Swashbuckle.Swagger;

namespace Swashbuckle.Swagger.XmlComments
{
    public class ApplyXmlTypeComments : IModelFilter
    {
        private const string TypeExpression = "/doc/members/member[@name='T:{0}']";
        private const string SummaryExpression = "summary";
        private const string PropertyExpression = "/doc/members/member[@name='P:{0}.{1}']";

        private readonly XPathNavigator _navigator;

        public ApplyXmlTypeComments(string xmlCommentsPath)
        {
            _navigator = new XPathDocument(xmlCommentsPath).CreateNavigator();
        }

        public ApplyXmlTypeComments(XPathNavigator xmlCommentsNavigator)
        {
            _navigator = xmlCommentsNavigator;
        }

        public void Apply(Schema model, ModelFilterContext context)
        {
            var typeNode = _navigator.SelectSingleNode(
                String.Format(TypeExpression, context.SystemType.XmlLookupName()));

            if (typeNode != null)
            {
                var summaryNode = typeNode.SelectSingleNode(SummaryExpression);
                if (summaryNode != null)
                    model.description = summaryNode.ExtractContent();
            }

            foreach (var entry in model.properties)
            {
                var jsonProperty = context.JsonObjectContract.Properties[entry.Key];
                if (jsonProperty == null) continue;

                ApplyPropertyComments(entry.Value, jsonProperty.PropertyInfo());
            }
        }

        private void ApplyPropertyComments(Schema propertySchema, PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) return;

            var propertyNode = _navigator.SelectSingleNode(
                String.Format(PropertyExpression, propertyInfo.DeclaringType.XmlLookupName(), propertyInfo.Name));
            if (propertyNode == null) return;

            var propSummaryNode = propertyNode.SelectSingleNode(SummaryExpression);
            if (propSummaryNode != null)
            {
                propertySchema.description = propSummaryNode.ExtractContent();
            }
        }
    }
}