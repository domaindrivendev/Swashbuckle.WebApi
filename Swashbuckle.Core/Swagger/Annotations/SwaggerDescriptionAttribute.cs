using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Swashbuckle.Swagger.Annotations
{
    public class SwaggerDescriptionAttribute : Attribute
    {
        public SwaggerDescriptionAttribute(string description = null)
        {
            this.Description = description;
        }

        public string Description { get; set; }
    }
}
