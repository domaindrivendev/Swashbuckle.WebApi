using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.Swagger
{
    public class TypeSystem
    {
        private static readonly Dictionary<Type, Func<DataType>> PrimitiveMappings = new Dictionary<Type, Func<DataType>>()
            {
                {typeof (Int16), () => new DataType {Type = "integer", Format = "int32"}},
                {typeof (UInt16), () => new DataType {Type = "integer", Format = "int32"}},
                {typeof (Int32), () => new DataType {Type = "integer", Format = "int32"}},
                {typeof (UInt32), () => new DataType {Type = "integer", Format = "int32"}},
                {typeof (Int64), () => new DataType {Type = "integer", Format = "int64"}},
                {typeof (UInt64), () => new DataType {Type = "integer", Format = "int64"}},
                {typeof (Single), () => new DataType {Type = "number", Format = "float"}},
                {typeof (Double), () => new DataType {Type = "number", Format = "double"}},
                {typeof (Decimal), () => new DataType {Type = "number", Format = "double"}},
                {typeof (String), () => new DataType {Type = "string", Format = null}},
                {typeof (Char), () => new DataType {Type = "string", Format = null}},
                {typeof (Byte), () => new DataType {Type = "string", Format = "byte"}},
                {typeof (Boolean), () => new DataType {Type = "boolean", Format = null}},
                {typeof (DateTime), () => new DataType {Type = "string", Format = "date-time"}},
                {typeof (DateTimeOffset), () => new DataType {Type = "string", Format = "date-time"}},
                // Can't infer anything from the types below - default to string primitives
                {typeof (object), () => new DataType{Type="string", Format = null}},
                {typeof (ExpandoObject), () => new DataType{Type="string", Format = null}},
                {typeof (JObject), () => new DataType{Type="string", Format = null}},
                {typeof (JToken), () => new DataType{Type="string", Format = null}},
                {typeof (HttpResponseMessage), () => new DataType{Type="string", Format = null}},
            };

        private readonly IDictionary<Type, Func<DataType>> _customMappings;
        private readonly IEnumerable<PolymorphicType> _polymorphicTypes;
        private readonly IEnumerable<IModelFilter> _modelFilters;
        private readonly IDictionary<Type, Model> _registeredModels;

        public TypeSystem(
            IDictionary<Type, Func<DataType>> customMappings,
            IEnumerable<PolymorphicType> polymorphicTypes,
            IEnumerable<IModelFilter> modelFilters)
        {
            _customMappings = customMappings;
            _polymorphicTypes = polymorphicTypes;
            _modelFilters = modelFilters;
            _registeredModels = new Dictionary<Type, Model>();
        }

        public DataType GetDataTypeFor(Type type)
        {
            var queue = new Queue<Type>(); // defer processing of complex types
            var dataType = CreateDataTypeFor(type, queue);

            // Work through the queue of complex types
            while (queue.Any())
            {
                var model = CreateModelFor(queue.Peek(), queue);
                _registeredModels.Add(queue.Dequeue(), model);
            }

            return dataType;
        }

        public IDictionary<string, Model> GetModels()
        {
            try
            {
                return _registeredModels.ToDictionary(entry => entry.Value.Id, entry => entry.Value);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException("Failed to generate Swagger models with unique Id's. Do you have multiple API types with the same class name?", ex);
            }
        }

        private DataType CreateDataTypeFor(Type type, Queue<Type> queue)
        {
            if (_customMappings.ContainsKey(type))
                return _customMappings[type]();

            if (PrimitiveMappings.ContainsKey(type))
                return PrimitiveMappings[type]();

            if (type.IsEnum)
                return new DataType { Type = "string", Enum = type.GetEnumNames() };

            Type innerType;
            if (type.IsNullable(out innerType))
                return CreateDataTypeFor(innerType, queue);

            Type itemType;
            if (type.IsEnumerable(out itemType))
            {
                if (itemType.IsEnumerable() && !PrimitiveMappings.ContainsKey(itemType))
                    throw new InvalidOperationException(
                        String.Format("Type {0} is not supported. Swagger does not support containers of containers", type));

                return new DataType { Type = "array", Items = CreateDataTypeFor(itemType, queue) };
            }

            // A complex type! If not already registered and not currently queued, queue it up
            if (!_registeredModels.ContainsKey(type) && !queue.Contains(type))
                queue.Enqueue(type);

            return new DataType { Ref = UniqueIdFor(type) };
        }

        private Model CreateModelFor(Type type, Queue<Type> queue)
        {
            // Ignore inherited properties if its an explicitly configured polymorphic sub type
            var polymorphicType = PolymorphicTypeFor(type);
            var bindingFlags = polymorphicType.IsBase
                ? BindingFlags.Instance | BindingFlags.Public
                : BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

            var propInfos = type.GetProperties(bindingFlags)
                .Where(propInfo => !propInfo.GetIndexParameters().Any())    // Ignore indexer properties
                .ToArray();

            var properties = propInfos.ToDictionary(
                propInfo => propInfo.Name,
                propInfo =>
                {
                    var property = new Property();
                    property.CopyValuesFrom(CreateDataTypeFor(propInfo.PropertyType, queue));
                    return property;
                });

            var required = propInfos.Where(propInfo => Attribute.IsDefined(propInfo, typeof (RequiredAttribute)))
                .Select(propInfo => propInfo.Name)
                .ToList();

            var subDataTypes = polymorphicType.SubTypes
                .Select(subType => CreateDataTypeFor(subType.Type, queue))
                .Select(subDataType => subDataType.Ref)
                .ToList();

            var dataType = new Model
            {
                Id = UniqueIdFor(type),
                Properties = properties,
                Required = required,
                SubTypes = subDataTypes,
                Discriminator = polymorphicType.Discriminator
            };

            foreach (var filter in _modelFilters)
            {
                filter.Apply(dataType, this, type);
            }

            return dataType;
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

        private PolymorphicType PolymorphicTypeFor(Type type)
        {
            var polymorphicType = _polymorphicTypes.SingleOrDefault(t => t.Type == type);
            if (polymorphicType != null) return polymorphicType;
            
            // Is it nested?
            foreach (var baseType in _polymorphicTypes)
            {
                polymorphicType = baseType.FindSubType(type);
                if (polymorphicType != null) return polymorphicType;
            }

            return new PolymorphicType(type, true);
        }
    }
}