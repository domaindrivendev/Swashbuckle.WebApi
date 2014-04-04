using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.Core.Swagger
{
    public class DataTypeGenerator
    {
        private static readonly Dictionary<Type, DataType> StaticMappings = new Dictionary<Type, DataType>()
            {
                {typeof (Int16), new DataType {Type = "integer", Format = "int32"}},
                {typeof (UInt16), new DataType {Type = "integer", Format = "int32"}},
                {typeof (Int32), new DataType {Type = "integer", Format = "int32"}},
                {typeof (UInt32), new DataType {Type = "integer", Format = "int32"}},
                {typeof (Int64), new DataType {Type = "integer", Format = "int64"}},
                {typeof (UInt64), new DataType {Type = "integer", Format = "int64"}},
                {typeof (Single), new DataType {Type = "number", Format = "float"}},
                {typeof (Double), new DataType {Type = "number", Format = "double"}},
                {typeof (Decimal), new DataType {Type = "number", Format = "double"}},
                {typeof (String), new DataType {Type = "string", Format = null}},
                {typeof (Char), new DataType {Type = "string", Format = null}},
                {typeof (Byte), new DataType {Type = "string", Format = "byte"}},
                {typeof (Boolean), new DataType {Type = "boolean", Format = null}},
                {typeof (DateTime), new DataType {Type = "string", Format = "date-time"}},
                {typeof (Object), new DataType{Type="string", Format = null}},
                {typeof (ExpandoObject), new DataType{Type="string", Format = null}},
                {typeof (JObject), new DataType{Type="string", Format = null}},
                {typeof (HttpResponseMessage), new DataType{Type="string", Format = null}}
            };

        private readonly IDictionary<Type, DataType> _customMappings;
        private readonly IEnumerable<PolymorphicType> _polymorphicTypes;
            
        public DataTypeGenerator(IDictionary<Type, DataType> customMappings, IEnumerable<PolymorphicType> polymorphicTypes)
        {
            if (customMappings == null)
                throw new ArgumentNullException("customMappings");

            _customMappings = customMappings;
            _polymorphicTypes = polymorphicTypes;
        }

        public DataType TypeToDataType(Type type, out IDictionary<string, DataType> complexModels)
        {
            // Track complex mappings, deferred mappings will have a null Value
            var complexMappings = new Dictionary<Type, DataType>();

            var rootDataType = CreateDataType(type, false, complexMappings);

            // Iterate through remaining deferred mappings
            while (complexMappings.ContainsValue(null))
            {
                var complexType = complexMappings.First(kvp => kvp.Value == null).Key;
                var spec = CreateDataType(complexType, false, complexMappings);
                complexMappings[complexType] = spec;
            }

            complexModels = complexMappings.ToDictionary((entry) => entry.Value.Id, (entry) => entry.Value);
            if (rootDataType.Type == "object")
                complexModels[rootDataType.Id] = rootDataType;

            return rootDataType;
        }

        private DataType CreateDataType(Type type, bool deferIfComplex, IDictionary<Type, DataType> complexMappings)
        {
            if (_customMappings.ContainsKey(type))
                return _customMappings[type];

            if (StaticMappings.ContainsKey(type))
                return StaticMappings[type];

            if (type.IsEnum)
                return new DataType { Type = "string", Enum = type.GetEnumNames() };

            Type innerType;
            if (type.IsNullable(out innerType))
                return CreateDataType(innerType, deferIfComplex, complexMappings);

            Type itemType;
            if (type.IsEnumerable(out itemType))
                return new DataType { Type = "array", Items = CreateDataType(itemType, true, complexMappings) };

            // Anthing else is complex

            if (deferIfComplex)
            {
                if (!complexMappings.ContainsKey(type))
                    complexMappings.Add(type, null);

                // Just return a reference for now
                return new DataType { Ref = UniqueIdFor(type) };
            }

            return CreateComplexDataType(type, complexMappings);
        }

        private DataType CreateComplexDataType(Type type, IDictionary<Type, DataType> complexMappings)
        {
            var propInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(propInfo => !propInfo.GetIndexParameters().Any())    // Ignore indexer properties
                .ToArray();

            var properties = propInfos
                .ToDictionary(propInfo => propInfo.Name, propInfo => CreateDataType(propInfo.PropertyType, true, complexMappings));

            var required = propInfos.Where(propInfo => Attribute.IsDefined(propInfo, typeof (RequiredAttribute)))
                .Select(propInfo => propInfo.Name)
                .ToList();

            var polymorphicType = PolymorphicTypeFor(type);
            var subDataTypes = polymorphicType.SubTypes
                .Select(subType => CreateDataType(subType.Type, true, complexMappings))
                .Select(subDataType => subDataType.Ref)
                .ToList();

            return new DataType
            {
                Id = UniqueIdFor(type),
                Type = "object",
                Properties = properties,
                Required = required,
                SubTypes = subDataTypes,
                Discriminator = polymorphicType.Discriminator
            };
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