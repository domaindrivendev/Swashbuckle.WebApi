using Swashbuckle.Swagger;

namespace Swashbuckle.AutoRestExtensions
{
    public static class SchemaExtensions
    {
        public static bool HasNullable(this Schema schema)
        {
            return (schema.vendorExtensions != null) &&
                schema.vendorExtensions.ContainsKey("x-nullable");
        }

        public static bool IsNullable(this Schema schema)
        {
            object value;
            return (schema.vendorExtensions != null) &&
                schema.vendorExtensions.TryGetValue("x-nullable", out value) &&
                (value is bool) &&
                (bool)value;
        }
    }
}
