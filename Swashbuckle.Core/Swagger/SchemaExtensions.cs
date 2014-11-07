using System;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.Swagger
{
    public static class SchemaExtensions
    {
        public static Schema WithValidationProperties(this Schema schema, JsonProperty jsonProperty)
        {
            var propInfo = jsonProperty.PropertyInfo();
            if (propInfo == null)
                return schema;

            foreach (var attribute in propInfo.GetCustomAttributes(false))
            {
                var regex = attribute as RegularExpressionAttribute;
                if (regex != null)
                    schema.pattern = regex.Pattern;

                var range = attribute as RangeAttribute;
                if (range != null)
                {
                    int maximum;
                    if (Int32.TryParse(range.Maximum.ToString(), out maximum))
                        schema.maximum = maximum;

                    int minimum;
                    if (Int32.TryParse(range.Minimum.ToString(), out minimum))
                        schema.minimum = minimum;
                }
            }

            return schema;
        }

        public static void PopulateFrom(this PartialSchema partialSchema, Schema schema)
        {
            if (schema == null) return;

            partialSchema.type = schema.type;
            partialSchema.format = schema.format;

            if (schema.items != null)
            {
                // TODO: Handle jagged primitive array and error on jagged object array
                partialSchema.items = new PartialSchema();
                partialSchema.items.PopulateFrom(schema.items);
            }

            partialSchema.@default = schema.@default;
            partialSchema.maximum = schema.maximum;
            partialSchema.exclusiveMaximum = schema.exclusiveMaximum;
            partialSchema.minimum = schema.minimum;
            partialSchema.exclusiveMinimum = schema.exclusiveMinimum;
            partialSchema.maxLength = schema.maxLength;
            partialSchema.minLength = schema.minLength;
            partialSchema.pattern = schema.pattern;
            partialSchema.maxItems = schema.maxItems;
            partialSchema.minItems = schema.minItems;
            partialSchema.uniqueItems = schema.uniqueItems;
            partialSchema.@enum = schema.@enum;
            partialSchema.multipleOf = schema.multipleOf;
        }
    }
}
