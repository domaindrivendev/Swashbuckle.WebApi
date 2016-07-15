using System;
using Swashbuckle.Swagger;

namespace Swashbuckle.Dummy.SwaggerExtensions
{
    public class ApplySchemaVendorExtensions : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (type.IsArray && type.GetElementType() != typeof(byte)) return; // Special case

            var nullableUnderlyingType = Nullable.GetUnderlyingType(type);

            schema.vendorExtensions.Add("x-type-dotnet", (nullableUnderlyingType ?? type).FullName);
            schema.vendorExtensions.Add("x-nullable", nullableUnderlyingType != null || !type.IsValueType);
        }
    }
}