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

namespace Swashbuckle.Swagger20
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
                {typeof (Guid), () => new Schema {type = "string"}},
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

        private readonly IEnumerable<ISchemaFilter> _schemaFilters;

        public SchemaRegistry(IEnumerable<ISchemaFilter> schemaFilters)
        {
            Definitions = new Dictionary<string, Schema>();
            _schemaFilters = schemaFilters;
        }

        public IDictionary<string, Schema> Definitions { get; private set; }

        public Schema FindOrRegister(Type type)
        {
            var referencedTypes = new Queue<KeyValuePair<string, Type>>();
            var rootSchema = CreateSchema(type, false, true, referencedTypes);

            while (referencedTypes.Any())
            {
                var next = referencedTypes.Dequeue();
                if (Definitions.ContainsKey(next.Key)) continue;

                Definitions.Add(next.Key, CreateSchema(next.Value, false, false, referencedTypes));
            }

            return rootSchema;
        }

        private Schema CreateSchema(
            Type type,
            bool refIfArray,
            bool refIfComplex,
            Queue<KeyValuePair<string, Type>> referencedTypes)
        {
            //if (_customMappings.ContainsKey(type))
            //    return _customMappings[type]();

            if (PrimitiveMappings.ContainsKey(type))
                return PrimitiveMappings[type]();

            if (type.IsEnum)
                return new Schema { type = "string", @enum = type.GetEnumNames() };

            Type innerType;
            if (type.IsNullable(out innerType))
                return CreateSchema(innerType, false, true, referencedTypes);

            Type itemType;
            if (type.IsEnumerable(out itemType) && !refIfArray)
                return new Schema { type = "array", items = CreateSchema(itemType, true, true, referencedTypes) };

            // None of the above so treat as a complex type
            if (!refIfComplex)
                return CreateComplexSchema(type, referencedTypes);

            // Only a ref was requested so defer the full schema generation
            var id = UniqueIdFor(type);
            referencedTypes.Enqueue(new KeyValuePair<string, Type>(id, type));
            return new Schema { @ref = "#/definitions/" + id };
        }

        private Schema CreateComplexSchema(Type type, Queue<KeyValuePair<string, Type>> referencedTypes)
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
                propInfo => CreateSchema(propInfo.PropertyType, false, true, referencedTypes)
                    .WithValidationProperties(propInfo));

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

            foreach (var filter in _schemaFilters)
            {
                filter.Apply(schema, this, type);
            }

            return schema;
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
