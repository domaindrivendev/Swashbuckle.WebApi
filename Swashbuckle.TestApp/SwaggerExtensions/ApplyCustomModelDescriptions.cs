using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Xml.XPath;
using Swashbuckle.Models;
namespace Swashbuckle.TestApp.SwaggerExtensions
{
    /// <summary>
    /// Read XML Documentation and apply it to the Model
    /// </summary>
    public class ApplyCustomModelDescriptions: IModelFilter
    {
        readonly XPathNavigator _documentNavigator;
        private const string TypeExpression = "/doc/members/member[@name='T:{0}']";
        private const string PropertyExpression = "/doc/members/member[@name='P:{0}.{1}']";

        public ApplyCustomModelDescriptions()
        {
            var xpath = new XPathDocument(HttpContext.Current.Server.MapPath("~/bin/Swashbuckle.TestApp.xml"));
            _documentNavigator = xpath.CreateNavigator();
        }
        
        #region IModelFilter Members

        public void Apply(Type type, ModelSpec modelSpec)
        {
            modelSpec.Description = GetDocumentation(type);
            foreach(KeyValuePair<string, ModelSpec> prop in modelSpec.Properties)
            {
                PropertyInfo propInfo = type.GetProperty(prop.Key);
                if (propInfo != null)
                {
                    prop.Value.Description = GetDocumentation(propInfo);
                }
            }
        }

        private string GetDocumentation(Type type)
        {
            var selectExpression = string.Format(TypeExpression, type.FullName);
            var node = _documentNavigator.SelectSingleNode(selectExpression);
            if (node != null)
            {
                var summaryNode = node.SelectSingleNode("summary");
                if (summaryNode != null)
                {
                    return summaryNode.Value.Trim();
                }
            }
            return "No documentation found";
        }
        
        private string GetDocumentation(PropertyInfo propertyInfo)
        {
            var selectExpression = string.Format(PropertyExpression, propertyInfo.DeclaringType.FullName, propertyInfo.Name);
            var node = _documentNavigator.SelectSingleNode(selectExpression);
            if (node != null)
            {
                var summaryNode = node.SelectSingleNode("summary");
                if (summaryNode != null)
                {
                    return summaryNode.Value.Trim();
                }
            }
            return "No documentation found";
        }
        #endregion
    }
}