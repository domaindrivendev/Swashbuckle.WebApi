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
    public class DataTypeRegistry
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
                {typeof (Object), () => new DataType{Type="string", Format = null}},
                {typeof (ExpandoObject), () => new DataType{Type="string", Format = null}},
                {typeof (JObject), () => new DataType{Type="string", Format = null}},
                {typeof (HttpResponseMessage), () => new DataType{Type="string", Format = null}}
            };

        private readonly IDictionary<Type, DataType> _complexMappings;
        private readonly Dictionary<Type, Func<DataType>> _customMappings;
        private readonly IEnumerable<PolymorphicType> _polymorphicTypes;
        private readonly IEnumerable<IModelFilter> _modelFilters;

        public DataTypeRegistry(Dictionary<Type, Func<DataType>> customMappings, IEnumerable<PolymorphicType> polymorphicTypes, IEnumerable<IModelFilter> modelFilters)
        {
            _complexMappings = new Dictionary<Type, DataType>();
            _customMappings = customMappings;
            _polymorphicTypes = polymorphicTypes;
            _modelFilters = modelFilters;
        }

        public DataType GetOrRegister(Type type)
        {
            // Defer processing of related models to avoid infinite recursion
            var deferredTypes = new Queue<Type>();

            var rootDataType = GetOrRegister(type, false, deferredTypes);

            // Process any remaining deferred type
            while (deferredTypes.Any())
            {
                var deferredType = deferredTypes.Dequeue();
                GetOrRegister(deferredType, false, deferredTypes);
            }

            return rootDataType;
        }

        public IDictionary<string, DataType> GetModels()
        {
            try
            {
                return _complexMappings.ToDictionary(entry => entry.Value.Id, entry => entry.Value);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException("Failed to generate Swagger models with unique Id's. Do you have multiple API types with the same class name?");
            }
        }

        private DataType GetOrRegister(Type type, bool deferIfComplex, Queue<Type> deferredTypes)
        {
            if (_customMappings.ContainsKey(type))
                return _customMappings[type]();

            if (PrimitiveMappings.ContainsKey(type))
                return PrimitiveMappings[type]();

            if (type.IsEnum)
                return new DataType { Type = "string", Enum = type.GetEnumNames() };

            Type innerType;
            if (type.IsNullable(out innerType))
                return GetOrRegister(innerType, deferIfComplex, deferredTypes);

            Type itemType;
            if (type.IsEnumerable(out itemType))
                return new DataType { Type = "array", Items = GetOrRegister(itemType, true, deferredTypes) };

            // Anthing else is complex
            if (deferIfComplex)
            {
                if (!_complexMappings.ContainsKey(type))
                    deferredTypes.Enqueue(type);

                // Just return a reference for now
                return new DataType { Ref = UniqueIdFor(type) };
            }

            return _complexMappings.GetOrAdd(type, () => CreateComplexDataType(type, deferredTypes));
        }

        private DataType CreateComplexDataType(Type type, Queue<Type> deferredTypes)
        {
            var propInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(propInfo => !propInfo.GetIndexParameters().Any())    // Ignore indexer properties
                .ToArray();

            var properties = propInfos
                .ToDictionary(propInfo => propInfo.Name, propInfo => GetOrRegister(propInfo.PropertyType, true, deferredTypes));

            var required = propInfos.Where(propInfo => Attribute.IsDefined(propInfo, typeof (RequiredAttribute)))
                .Select(propInfo => propInfo.Name)
                .ToList();

            var polymorphicType = PolymorphicTypeFor(type);
            var subDataTypes = polymorphicType.SubTypes
                .Select(subType => GetOrRegister(subType.Type, true, deferredTypes))
                .Select(subDataType => subDataType.Ref)
                .ToList();

            var dataType = new DataType
            {
                Id = UniqueIdFor(type),
                Type = "object",
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