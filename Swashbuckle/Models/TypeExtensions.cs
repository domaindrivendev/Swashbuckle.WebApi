using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Schema;

namespace Swashbuckle.Models
{
    public class SwaggerTypeDescriptor
    {
        public SwaggerTypeCategory Category { get; set; }
        public string Name { get; set; }
        public string Format { get; set; }
        public IEnumerable<string> Enum { get; set; }
        public JsonSchema JsonSchema { get; set; }
        public SwaggerTypeDescriptor ContainedType { get; set; }
    }

    public enum SwaggerTypeCategory
    {
        Unknown,
        Primitive,
        Complex,
        Container
    }

    public static class TypeExtensions
    {
        public static SwaggerTypeDescriptor ToSwaggerType(this Type type)
        {
            return type.ToSwaggerType(null);
        }

        public static SwaggerTypeDescriptor ToSwaggerType(this Type type, IDictionary<string, string> customTypeMappings)
        {
            if (type == typeof (HttpResponseMessage))
                return new SwaggerTypeDescriptor {Category = SwaggerTypeCategory.Unknown};

            if (type == null)
                return new SwaggerTypeDescriptor {Category = SwaggerTypeCategory.Primitive, Name = "void"};

            var primitiveTypes = new Dictionary<string, string[]>
                {
                    {"Int32",       new[]{ "integer", "int32" }},
                    {"Int64",       new[]{ "integer", "int64" }},
                    {"Single",      new[]{ "number", "float" }},
                    {"Double",      new[]{ "number", "double" }},
                    {"String",      new[]{ "string", null }},
                    {"Byte",        new[]{ "string", "byte" }},
                    {"Boolean",     new[]{ "boolean", null }},
                    {"DateTime",    new[]{ "string", "date-time" }}
                };

            if (primitiveTypes.ContainsKey(type.Name))
            {
                var tuple = primitiveTypes[type.Name];
                return new SwaggerTypeDescriptor {Category = SwaggerTypeCategory.Primitive, Name = tuple[0], Format = tuple[1]};
            }

            IEnumerable<string> values;
            if (type.IsEnum(out values))
            {
                return new SwaggerTypeDescriptor {Category = SwaggerTypeCategory.Primitive, Name = "string", Enum = values};
            }

            Type nullableType;
            if (type.IsNullable(out nullableType))
            {
                return nullableType.ToSwaggerType();
            }

            Type containedType;
            if (type.IsEnumerable(out containedType))
            {
                var name = String.Format("array[{0}]", containedType.Name);
                return new SwaggerTypeDescriptor { Category = SwaggerTypeCategory.Container, Name = name, ContainedType = containedType.ToSwaggerType()};
            }

            return new SwaggerTypeDescriptor {Category = SwaggerTypeCategory.Complex, Name = type.Name};
        }

        internal static bool IsEnum(this Type type, out IEnumerable<string> values)
        {
            if (!type.IsEnum)
            {
                values = null;
                return false;
            }

            values = type.GetEnumNames();
            return true;
        }

        internal static bool IsNullable(this Type type, out Type nullableType)
        {
            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (isNullable)
            {
                nullableType = type.GetGenericArguments().Single();
                return true;
            }

            nullableType = null;
            return false;
        }

        internal static bool IsEnumerable(this Type type, out Type containedType)
        {
            var enumerable = type.GetInterfaces()
                .Union(new[] {type})
                .SingleOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof (IEnumerable<>));

            if (enumerable == null)
            {
                containedType = null;
                return false;
            }

            containedType = enumerable.GetGenericArguments()[0];
            return true;
        }
    }
}