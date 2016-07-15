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
        private const string MemberXPath = "/doc/members/member[@name='{0}']";
        private const string SummaryTag = "summary";

        private readonly XPathNavigator _navigator;

        public ApplyXmlTypeComments(string xmlCommentsPath)
        {
            _navigator = new XPathDocument(xmlCommentsPath).CreateNavigator();
        }

        public void Apply(Schema model, ModelFilterContext context)
        {
            var commentId = XmlCommentsIdHelper.GetCommentIdForType(context.SystemType);
            var typeNode = _navigator.SelectSingleNode(string.Format(MemberXPath, commentId));

            if (typeNode != null)
            {
                var summaryNode = typeNode.SelectSingleNode(SummaryTag);
                if (summaryNode != null)
                    model.description = summaryNode.ExtractContent();
            }

            if (model.properties != null)
            {
                foreach (var entry in model.properties)
                {
                    var jsonProperty = context.JsonObjectContract.Properties[entry.Key];
                    if (jsonProperty == null) continue;

                    ApplyPropertyComments(entry.Value, jsonProperty.PropertyInfo());
                }
            }
        }

        private void ApplyPropertyComments(Schema propertySchema, PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) return;

            var commentId = XmlCommentsIdHelper.GetCommentIdForProperty(propertyInfo);
            var propertyNode = _navigator.SelectSingleNode(string.Format(MemberXPath, commentId));
            if (propertyNode == null) return;

            var propSummaryNode = propertyNode.SelectSingleNode(SummaryTag);
            if (propSummaryNode != null)
            {
                propertySchema.description = propSummaryNode.ExtractContent();
            }
        }
    }
}