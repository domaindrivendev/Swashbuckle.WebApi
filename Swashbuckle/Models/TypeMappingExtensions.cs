using System;

namespace Swashbuckle.Models
{
    internal static class TypeMappingExtensions
    {
        internal static AllowableValuesSpec AllowableValues(this Type type)
        {
            Type innerType;
            if (type.IsNullableType(out innerType))
                return innerType.AllowableValues();

            if (!type.IsEnum)
                return null;

            return new EnumeratedValuesSpec
                {
                    values = type.GetEnumNames()
                };
        }

        internal static ModelPropertySpec ToModelPropertySpec(this Type type)
        {
            return new ModelPropertySpec
                {
                    type = type.ToSwaggerType(),
                    required = true,
                    allowableValues = type.AllowableValues()
                };
        }
    }
}