using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger;

namespace Swashbuckle.Swagger2
{
    public class SchemaRegistry
    {
        private static readonly Dictionary<Type, Func<Schema>> PrimitiveMappings = new Dictionary<Type, Func<Schema>>()
            {
                {typeof (Int16), () => new Schema {type = "integer", format = "int32"}},
                {typeof (UInt16), () => new Schema {type = "integer", format = "int32"}},
                {typeof (Int32), () => new Schema {type = "integer", format = "int32"}},
                {typeof (UInt32), () => new Schema {type = "integer", format = "int32"}},
                {typeof (Int64), () => new Schema {type = "integer", format = "int64"}},
                {typeof (UInt64), () => new Schema {type = "integer", format = "int64"}},
                {typeof (Single), () => new Schema {type = "number", format = "float"}},
                {typeof (Double), () => new Schema {type = "number", format = "double"}},
                {typeof (Decimal), () => new Schema {type = "number", format = "double"}},
                {typeof (String), () => new Schema {type = "string"}},
                {typeof (Char), () => new Schema {type = "string"}},
                {typeof (Byte), () => new Schema {type = "string", format = "byte"}},
                {typeof (Boolean), () => new Schema {type = "boolean"}},
                {typeof (DateTime), () => new Schema {type = "string", format = "date-time"}},
                {typeof (DateTimeOffset), () => new Schema {type = "string", format = "date-time"}},
                // Can't infer anything from the types below - default to string primitives
                {typeof (object), () => new Schema {type="string"}},
                {typeof (ExpandoObject), () => new Schema {type="string"}},
                {typeof (JObject), () => new Schema {type="string"}},
                {typeof (JToken), () => new Schema {type="string"}},
                {typeof (HttpResponseMessage), () => new Schema {type="string"}},
            };

        public SchemaRegistry()
        {
            Definitions = new Dictionary<string, Schema>();
        }

        public IDictionary<string, Schema> Definitions { get; private set; }

        public Schema FindOrRegister(Type type)
        {
            var queue = new Queue<Type>(); // defer processing of complex types
            var schema = CreateSimpleSchemaFor(type, queue);

            while (queue.Any())
            {
                RegisterComplexSchemaFor(queue.Peek(), queue);
                queue.Dequeue();
            }

            return schema;
        }

        private Schema CreateSimpleSchemaFor(Type type, Queue<Type> queue)
        {
            //if (_customMappings.ContainsKey(type))
            //    return _customMappings[type]();

            if (PrimitiveMappings.ContainsKey(type))
                return PrimitiveMappings[type]();

            if (type.IsEnum)
                return new Schema { type = "string", @enum = type.GetEnumNames() };

            Type innerType;
            if (type.IsNullable(out innerType))
                return CreateSimpleSchemaFor(innerType, queue);

            Type itemType;
            if (type.IsEnumerable(out itemType))
                return new Schema { type = "array", items = CreateSimpleSchemaFor(itemType, queue) };

            // A complex type! If not already registered and not currently queued, queue it up
            var reference = "#/definitions/" + UniqueIdFor(type);
            if (!Definitions.ContainsKey(reference) && !queue.Contains(type))
                queue.Enqueue(type);

            return new Schema { @ref = reference };
        }

        private void RegisterComplexSchemaFor(Type type, Queue<Type> queue)
        {
            // Ignore inherited properties if its an explicitly configured polymorphic sub type
            //var polymorphicType = PolymorphicTypeFor(type);
            //var bindingFlags = polymorphicType.IsBase
            //    ? BindingFlags.Instance | BindingFlags.Public
            //    : BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;

            var propInfos = type.GetProperties(bindingFlags)
                .Where(propInfo => !propInfo.GetIndexParameters().Any())    // Ignore indexer properties
                .ToArray();

            var properties = propInfos.ToDictionary(
                propInfo => propInfo.Name,
                propInfo => CreateSimpleSchemaFor(propInfo.PropertyType, queue).WithValidations(propInfo));

            var required = propInfos.Where(propInfo => Attribute.IsDefined(propInfo, typeof(RequiredAttribute)))
                .Select(propInfo => propInfo.Name)
                .ToList();

            //var subDataTypes = polymorphicType.SubTypes
            //    .Select(subType => CreateDataTypeFor(subType.Type, queue))
            //    .Select(subDataType => subDataType.Ref)
            //    .ToList();

            var schema = new Schema
            {
                required = required,
                properties = properties,
                type = "object"
            };

            Definitions[UniqueIdFor(type)] = schema;
        }

        private static string UniqueIdFor(Type type)
        {
            if (type.IsGenericType)
            {
                var genericArguments = type.GetGenericArguments()
                    .Select(UniqueIdFor)
                    .ToArray();

                var builder = new StringBuilder(type.Name);

                return builder
                    .Replace(String.Format("`{0}", genericArguments.Count()), String.Empty)
                    .Append(String.Format("[{0}]", String.Join(",", genericArguments).TrimEnd(',')))
                    .ToString();
            }

            return type.Name;
        }

        //private PolymorphicType PolymorphicTypeFor(Type type)
        //{
        //    var polymorphicType = _polymorphicTypes.SingleOrDefault(t => t.Type == type);
        //    if (polymorphicType != null) return polymorphicType;
            
        //    // Is it nested?
        //    foreach (var baseType in _polymorphicTypes)
        //    {
        //        polymorphicType = baseType.FindSubType(type);
        //        if (polymorphicType != null) return polymorphicType;
        //    }

        //    return new PolymorphicType(type, true);
        //}
    }
}
