using System.Collections.Generic;
using System.Web.Http.Description;
using System.Xml.Linq;
using Swashbuckle.Core.Swagger;

namespace Swashbuckle.TestApp.Core.SwaggerExtensions
{
    public class ExtractXmlComments : IOperationSpecFilter
    {
        public void Apply(OperationSpec operationSpec, Dictionary<string, ModelSpec> complexModels, ModelSpecGenerator modelSpecGenerator, ApiDescription apiDescription)
        {
            var descriptionXml = XElement.Parse(apiDescription.Documentation);

            var summary = descriptionXml.Element("summary");
            operationSpec.Summary = summary != null ? summary.Value : descriptionXml.Value;

            var notes = descriptionXml.Element("remarks");
            if (notes != null)
                operationSpec.Notes = notes.Value;
        }
    }
}