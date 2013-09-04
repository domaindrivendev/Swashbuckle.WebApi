using System;
using System.Web.Http.Description;
using System.Xml;
using System.Xml.Linq;
using Swashbuckle.Models;

namespace Swashbuckle.TestApp.SwaggerFilters
{
    public class AddXmlCommentsParsingFilter : IOperationSpecFilter
    {
        public void Apply(ApiDescription apiDescription, ApiOperationSpec operationSpec)
        {
            try
            {
                var descriptionXml = XElement.Parse(apiDescription.Documentation);

                var notes = descriptionXml.Element("remarks");
                if (notes != null)
                    operationSpec.notes = notes.Value;

                foreach (var error in descriptionXml.Elements("response"))
                {
                    operationSpec.errorResponses.Add(new ApiErrorResponseSpec() { code = Convert.ToInt32(error.Attribute("code").Value), reason = error.Value });
                }

                var summary = descriptionXml.Element("summary");
                operationSpec.summary = summary != null ? summary.Value : descriptionXml.Value;
            }
            catch (XmlException) { } // sorry, found no other reliable way to tell if this is xml or not
        }
    }
}