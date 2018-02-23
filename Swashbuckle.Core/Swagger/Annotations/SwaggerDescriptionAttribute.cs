using System;

namespace Swashbuckle.Swagger.Annotations
{
    public class SwaggerDescriptionAttribute : Attribute
    {
        public SwaggerDescriptionAttribute(string description = null, string summary = null)
        {
            Description = description;
            Summary = summary;
        }

        public string Description { get; set; }
        public string Summary { get; set; }
    }
}
