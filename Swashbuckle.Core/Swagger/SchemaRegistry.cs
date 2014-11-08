using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.Swagger
{
    public class SchemaRegistry
    {
        public static readonly Dictionary<Type, Func<Schema>> PrimitiveMappings = new Dictionary<Type, Func<Schema>>()
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
                {typeof (SByte), () => new Schema {type = "string", format = "byte"}},
                {typeof (Guid), () => new Schema {type = "string"}},
                {typeof (Boolean), () => new Schema {type = "boolean"}},
                {typeof (DateTime), () => new Schema {type = "string", format = "date-time"}},
                {typeof (DateTimeOffset), () => new Schema {type = "string", format = "date-time"}}
            };

        private static readonly IEnumerable<Type> HttpTypes = new[]
            {
                typeof(HttpRequestMessage),
                typeof(HttpResponseMessage),
                typeof(IHttpActionResult)
            };

        private readonly IContractResolver _contractResolver;
        private readonly IEnumerable<ISchemaFilter> _schemaFilters;

        public SchemaRegistry(IContractResolver contractResolver, IEnumerable<ISchemaFilter> schemaFilters)
        {
            _contractResolver = contractResolver;
            _schemaFilters = schemaFilters;

            Definitions = new Dictionary<string, Schema>(StringComparer.OrdinalIgnoreCase);
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
            if (PrimitiveMappings.ContainsKey(type))
                return PrimitiveMappings[type]();

            if (type.IsEnum)
                return new Schema { type = "string", @enum = type.GetEnumNames() };

            Type innerType;
            if (type.IsNullable(out innerType))
                return CreateSchema(innerType, false, true, referencedTypes);

            // Non-primitive - utilize the Json contract resolver
            var contract = _contractResolver.ResolveContract(type);

            if (contract is JsonArrayContract)
            {
                return refIfArray
                    ? CreateRefSchema(type, referencedTypes)
                    : CreateArraySchema((JsonArrayContract)contract, referencedTypes);
            }

            if (contract is JsonObjectContract && !HttpTypes.Contains(type))
            {
                return refIfComplex
                    ? CreateRefSchema(type, referencedTypes)
                    : CreateComplexSchema((JsonObjectContract)contract, referencedTypes);
            }

            // Falback, describe anything else as a "blank canvas" object
            return CreateSchema(typeof(object), refIfArray, refIfComplex, referencedTypes);
        }

        private Schema CreateRefSchema(Type type, Queue<KeyValuePair<string, Type>> referencedTypes)
        {
            var id = UniqueIdFor(type);
            referencedTypes.Enqueue(new KeyValuePair<string, Type>(id, type));
            return new Schema { @ref = "#/definitions/" + id };
        }

        private Schema CreateArraySchema(JsonArrayContract arrayContract, Queue<KeyValuePair<string, Type>> referencedTypes)
        {
            return new Schema
                {
                    type = "array",
                    items = CreateSchema(arrayContract.CollectionItemType, true, true, referencedTypes)
                };
        }

        private Schema CreateComplexSchema(JsonObjectContract objectContract, Queue<KeyValuePair<string, Type>> referencedTypes)
        {
            var properties = objectContract.Properties.Where(p => !p.Ignored).ToDictionary(
                prop => prop.PropertyName,
                prop => CreateSchema(prop.PropertyType, false, true, referencedTypes)
                    .WithValidationProperties(prop));

            var required = objectContract.Properties.Where(prop => prop.IsRequired())
                .Select(propInfo => propInfo.PropertyName)
                .ToList();

            var schema = new Schema
            {
                required = required,
                properties = properties,
                type = "object"
            };

            foreach (var filter in _schemaFilters)
            {
                filter.Apply(schema, this, objectContract.UnderlyingType);
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
    }
}
