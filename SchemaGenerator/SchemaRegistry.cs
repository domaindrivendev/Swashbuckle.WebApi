using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchemaGenerator
{
    public class SchemaRegistry
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        private readonly IContractResolver _contractResolver;

        private IDictionary<Type, SchemaInfo> _referencedTypes;
        private class SchemaInfo
        {
            public string SchemaId;
            public Schema Schema;
        }

        public SchemaRegistry()
        {
            _jsonSerializerSettings = new JsonSerializerSettings();
            _contractResolver = new DefaultContractResolver();
            _referencedTypes = new Dictionary<Type, SchemaInfo>();
            Definitions = new Dictionary<string, Schema>();
        }

        public Schema GetOrRegister(Type type)
        {
            var schema = CreateInlineSchema(type);

            // Ensure Schema's have been fully generated for all referenced types
            while (_referencedTypes.Any(entry => entry.Value.Schema == null))
            {
                var typeMapping = _referencedTypes.First(entry => entry.Value.Schema == null);
                var schemaInfo = typeMapping.Value;

                schemaInfo.Schema = CreateDefinitionSchema(typeMapping.Key);
                Definitions.Add(schemaInfo.SchemaId, schemaInfo.Schema);
            }

            return schema;
        }

        public IDictionary<string, Schema> Definitions { get; private set; }

        private Schema CreateInlineSchema(Type type)
        {
            var jsonContract = _contractResolver.ResolveContract(type);

            if (jsonContract is JsonPrimitiveContract)
                return CreatePrimitiveSchema((JsonPrimitiveContract)jsonContract);

            var dictionaryContract = jsonContract as JsonDictionaryContract;
            if (dictionaryContract != null)
                return dictionaryContract.IsSelfReferencing()
                    ? CreateRefSchema(type)
                    : CreateDictionarySchema(dictionaryContract);

            var arrayContract = jsonContract as JsonArrayContract;
            if (arrayContract != null)
                return arrayContract.IsSelfReferencing()
                    ? CreateRefSchema(type)
                    : CreateArraySchema(arrayContract);

            var objectContract = jsonContract as JsonObjectContract;
            if (objectContract != null && objectContract.IsInferrable())
                return CreateRefSchema(type);

            // Fallback to abstract "object"
            return CreateRefSchema(typeof(object));
        }

        private Schema CreateDefinitionSchema(Type type)
        {
            var jsonContract = _contractResolver.ResolveContract(type);

            if (jsonContract is JsonDictionaryContract)
                return CreateDictionarySchema((JsonDictionaryContract)jsonContract);

            if (jsonContract is JsonArrayContract)
                return CreateArraySchema((JsonArrayContract)jsonContract);

            if (jsonContract is JsonObjectContract)
                return CreateObjectSchema((JsonObjectContract)jsonContract);

            throw new InvalidOperationException(
                String.Format("Unsupported type - {0} for Defintitions. Must be Dictionary, Array or Object", type));
        }

        private Schema CreatePrimitiveSchema(JsonPrimitiveContract primitiveContract)
        {
            var type = Nullable.GetUnderlyingType(primitiveContract.UnderlyingType) ?? primitiveContract.UnderlyingType;

            if (type.IsEnum)
                return CreateEnumSchema(primitiveContract, type);

            switch (type.FullName)
            {
                case "System.Int16":
                case "System.UInt16":
                case "System.Int32":
                case "System.UInt32":
                    return new Schema { type = "integer", format = "int32" };
                case "System.Int64":
                case "System.UInt64":
                    return new Schema { type = "integer", format = "int64" };
                case "System.Single":
                    return new Schema { type = "number", format = "float" };
                case "System.Double":
                case "System.Decimal":
                    return new Schema { type = "number", format = "double" };
                case "System.Byte":
                case "System.SByte":
                    return new Schema { type = "string", format = "byte" };
                case "System.Boolean":
                    return new Schema { type = "boolean" };
                case "System.DateTime":
                case "System.DateTimeOffset":
                    return new Schema { type = "string", format = "date-time" };
                default:
                    return new Schema { type = "string" };
            }
        }

        private Schema CreateEnumSchema(JsonPrimitiveContract primitiveContract, Type type)
        {
            var stringEnumConverter = primitiveContract.Converter as StringEnumConverter
                ?? _jsonSerializerSettings.Converters.OfType<StringEnumConverter>().FirstOrDefault();

            if (stringEnumConverter != null)
            {
                var camelCase = (stringEnumConverter != null && stringEnumConverter.CamelCaseText);

                return new Schema
                {
                    type = "string",
                    @enum = camelCase
                        ? type.GetEnumNames().Select(name => name.ToCamelCase()).ToArray()
                        : type.GetEnumNames()
                };
            }

            return new Schema
            {
                type = "integer",
                format = "int32",
                @enum = type.GetEnumValues().Cast<object>().ToArray()
            };
        }

        private Schema CreateDictionarySchema(JsonDictionaryContract dictionaryContract)
        {
            var valueType = dictionaryContract.DictionaryValueType ?? typeof(object);
            return new Schema
            {
                type = "object",
                additionalProperties = CreateInlineSchema(valueType)
            };
        }

        private Schema CreateArraySchema(JsonArrayContract arrayContract)
        {
            var itemType = arrayContract.CollectionItemType ?? typeof(object);
            return new Schema
            {
                type = "array",
                items = CreateInlineSchema(itemType)
            };
        }

        private Schema CreateObjectSchema(JsonObjectContract jsonContract)
        {
            var properties = jsonContract.Properties
                .Where(p => !p.Ignored)
                .Where(p => !p.IsObsolete())
                .ToDictionary(
                    prop => prop.PropertyName,
                    prop => CreateInlineSchema(prop.PropertyType).WithValidationProperties(prop)
                );

            var required = jsonContract.Properties.Where(prop => prop.IsRequired())
                .Select(propInfo => propInfo.PropertyName)
                .ToList();

            var schema = new Schema
            {
                required = required.Any() ? required : null, // required can be null but not empty
                properties = properties,
                type = "object"
            };

            //foreach (var filter in _schemaFilters)
            //{
            //    filter.Apply(schema, this, jsonContract.UnderlyingType);
            //}

            //// NOTE: In next major version, _modelFilters will completely replace _schemaFilters
            //var modelFilterContext = new ModelFilterContext(jsonContract.UnderlyingType, jsonContract, this);
            //foreach (var filter in _modelFilters)
            //{
            //    filter.Apply(schema, modelFilterContext);
            //}

            return schema;
        }

        private Schema CreateRefSchema(Type type)
        {
            if (!_referencedTypes.ContainsKey(type))
            {
                var schemaId = type.FriendlyId();
                if (_referencedTypes.Any(entry => entry.Value.SchemaId == schemaId))
                {
                    var conflictingType = _referencedTypes.First(entry => entry.Value.SchemaId == schemaId).Key;
                    throw new InvalidOperationException(String.Format(
                        "Conflicting schemaIds: Duplicate schemaIds detected for types {0} and {1}. " +
                        "See the config setting - \"UseFullTypeNameInSchemaIds\" for a potential workaround",
                        type.FullName, conflictingType.FullName));
                }

                _referencedTypes.Add(type, new SchemaInfo { SchemaId = schemaId });
            }

            return new Schema { @ref = "#/definitions/" + _referencedTypes[type].SchemaId };
        }
    }
}