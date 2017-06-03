using System;
using Swashbuckle.Swagger;

namespace Swashbuckle.AutoRestExtensions
{
    public class TypeFormatSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            // Refine well known types
            switch (type.FullName)
            {
                case "System.Char":
                    schema.format = "char";
                    break;
                case "System.Decimal":
                    schema.format = "decimal";
                    break;
                case "System.TimeSpan":
                    schema.format = "duration";
                    break;
            }
        }
    }
}
