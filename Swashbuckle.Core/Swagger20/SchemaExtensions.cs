using System.Reflection;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Swashbuckle.Swagger20
{
    public static class SchemaExtensions
    {
        public static Schema WithValidationProperties(this Schema schema, PropertyInfo context)
        {
            foreach (var attribute in context.GetCustomAttributes(false))
            {
                var regex = attribute as RegularExpressionAttribute;
                if (regex != null)
                    schema.pattern = regex.Pattern;

                var range = attribute as RangeAttribute;
                if (range != null)
                {
                    schema.maximum = range.Maximum;
                    schema.minimum = range.Minimum;
                }
            }

            return schema;
        }
    }
}
