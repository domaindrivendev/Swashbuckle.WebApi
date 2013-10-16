using System;
using System.Web.Http.Description;
using System.Xml.Linq;
using Swashbuckle.Models;

namespace Swashbuckle.TestApp.Api.SwaggerExtensions
{
    public class ExtractXmlComments : IOperationSpecFilter
    {
        public void Apply(ApiDescription apiDescription, OperationSpec operationSpec)
        {
            var descriptionXml = XElement.Parse(apiDescription.Documentation);

            var summary = descriptionXml.Element("summary");
            operationSpec.Summary = summary != null ? summary.Value : descriptionXml.Value;

            var notes = descriptionXml.Element("remarks");
            if (notes != null)
                operationSpec.Notes = notes.Value;

            foreach (var error in descriptionXml.Elements("response"))
            {
                operationSpec.ResponseMessages.Add(new ResponseMessageSpec() { Code = Convert.ToInt32(error.Attribute("code").Value), Message = error.Value });
            }
        }
    }
}