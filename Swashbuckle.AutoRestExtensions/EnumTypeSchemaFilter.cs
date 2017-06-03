using System;
using Swashbuckle.Swagger;

namespace Swashbuckle.AutoRestExtensions
{
    public class EnumTypeSchemaFilter : ISchemaFilter
    {
        public EnumTypeSchemaFilter()
        {
        }

        public EnumTypeSchemaFilter(bool modelAsString)
        {
            _modelAsString = modelAsString;
        }

        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type.IsEnum)
            {
                // Add enum type information
                schema.vendorExtensions.Add("x-ms-enum", new
                {
                    name = type.Name,
                    modelAsString = _modelAsString
                });
            }
        }

        private readonly bool _modelAsString;
    }
}
