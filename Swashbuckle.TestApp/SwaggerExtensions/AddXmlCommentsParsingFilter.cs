using System;
using System.Web.Http.Description;
using System.Xml;
using System.Xml.Linq;
using Swashbuckle.Models;

namespace Swashbuckle.TestApp.SwaggerExtensions
{
    public class AddXmlCommentsParsingFilter : IOperationSpecFilter
    {
        public void Apply(ApiDescription apiDescription, OperationSpec operationSpec)
        {
            try
            {
                var descriptionXml = XElement.Parse(apiDescription.Documentation);

                var notes = descriptionXml.Element("remarks");
                if (notes != null)
                    operationSpec.Notes = notes.Value;

                foreach (var error in descriptionXml.Elements("response"))
                {
                    operationSpec.ResponseMessages.Add(new ResponseMessageSpec() { Code = Convert.ToInt32(error.Attribute("code").Value), Message = error.Value });
                }

                var summary = descriptionXml.Element("summary");
                operationSpec.Summary = summary != null ? summary.Value : descriptionXml.Value;
            }
            catch (XmlException) { } // sorry, found no other reliable way to tell if this is xml or not
        }
    }
}