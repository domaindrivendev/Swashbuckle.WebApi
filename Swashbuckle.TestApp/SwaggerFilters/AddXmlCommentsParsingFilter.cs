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

                var summary = descriptionXml.Element("summary");
                operationSpec.summary = summary != null ? summary.Value : descriptionXml.Value;
            }
            catch (XmlException) { } // sorry, found no other reliable way to tell if this is xml or not
        }
    }
}