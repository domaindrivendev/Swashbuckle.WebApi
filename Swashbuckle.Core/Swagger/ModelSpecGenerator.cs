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
    public class ModelSpecGenerator
    {
        private static readonly Dictionary<Type, ModelSpec> StaticMappings = new Dictionary<Type, ModelSpec>()
            {
                {typeof (Int16), new ModelSpec {Type = "integer", Format = "int32"}},
                {typeof (UInt16), new ModelSpec {Type = "integer", Format = "int32"}},
                {typeof (Int32), new ModelSpec {Type = "integer", Format = "int32"}},
                {typeof (UInt32), new ModelSpec {Type = "integer", Format = "int32"}},
                {typeof (Int64), new ModelSpec {Type = "integer", Format = "int64"}},
                {typeof (UInt64), new ModelSpec {Type = "integer", Format = "int64"}},
                {typeof (Single), new ModelSpec {Type = "number", Format = "float"}},
                {typeof (Double), new ModelSpec {Type = "number", Format = "double"}},
                {typeof (Decimal), new ModelSpec {Type = "number", Format = "double"}},
                {typeof (String), new ModelSpec {Type = "string", Format = null}},
                {typeof (Char), new ModelSpec {Type = "string", Format = null}},
                {typeof (Byte), new ModelSpec {Type = "string", Format = "byte"}},
                {typeof (Boolean), new ModelSpec {Type = "boolean", Format = null}},
                {typeof (DateTime), new ModelSpec {Type = "string", Format = "date-time"}},
                {typeof (Object), new ModelSpec{Type="string", Format = null}},
                {typeof (ExpandoObject), new ModelSpec{Type="string", Format = null}},
                {typeof (JObject), new ModelSpec{Type="string", Format = null}},
                {typeof (HttpResponseMessage), new ModelSpec{Type="string", Format = null}}
            };

        private readonly IDictionary<Type, ModelSpec> _customMappings;
        private readonly IEnumerable<PolymorphicType> _polymorphicTypes;
            
        public ModelSpecGenerator(IDictionary<Type, ModelSpec> customMappings, IEnumerable<PolymorphicType> polymorphicTypes)
        {
            if (customMappings == null)
                throw new ArgumentNullException("customMappings");

            _customMappings = customMappings;
            _polymorphicTypes = polymorphicTypes;
        }

        public ModelSpec TypeToModelSpec(Type type, out IDictionary<string, ModelSpec> complexModels)
        {
            // Track complex mappings, deferred mappings will have a null ModelSpec
            var complexTypeMappings = new Dictionary<Type, ModelSpec>();

            var rootSpec = CreateSpecFor(type, false, complexTypeMappings);

            // Iterate through remaining deferred mappings
            while (complexTypeMappings.ContainsValue(null))
            {
                var complexType = complexTypeMappings.First(kvp => kvp.Value == null).Key;
                var spec = CreateSpecFor(complexType, false, complexTypeMappings);
                complexTypeMappings[complexType] = spec;
            }

            complexModels = complexTypeMappings.ToDictionary((entry) => entry.Value.Id, (entry) => entry.Value);
            if (rootSpec.Type == "object")
                complexModels[rootSpec.Id] = rootSpec;

            return rootSpec;
        }

        private ModelSpec CreateSpecFor(Type type, bool deferIfComplex, IDictionary<Type, ModelSpec> complexTypeMappings)
        {
            if (_customMappings.ContainsKey(type))
                return _customMappings[type];

            if (StaticMappings.ContainsKey(type))
                return StaticMappings[type];

            if (type.IsEnum)
                return new ModelSpec { Type = "string", Enum = type.GetEnumNames() };

            Type innerType;
            if (type.IsNullable(out innerType))
                return CreateSpecFor(innerType, deferIfComplex, complexTypeMappings);

            Type itemType;
            if (type.IsEnumerable(out itemType))
                return new ModelSpec { Type = "array", Items = CreateSpecFor(itemType, true, complexTypeMappings) };

            // Anthing else is complex

            if (deferIfComplex)
            {
                if (!complexTypeMappings.ContainsKey(type))
                    complexTypeMappings.Add(type, null);

                // Just return a reference for now
                return new ModelSpec { Ref = UniqueIdFor(type) };
            }

            return CreateComplexSpecFor(type, complexTypeMappings);
        }

        private ModelSpec CreateComplexSpecFor(Type type, IDictionary<Type, ModelSpec> complexTypeMappings)
        {
            var propInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(propInfo => !propInfo.GetIndexParameters().Any())    // Ignore indexer properties
                .ToArray();

            var propSpecs = propInfos
                .ToDictionary(propInfo => propInfo.Name, propInfo => CreateSpecFor(propInfo.PropertyType, true, complexTypeMappings));

            var required = propInfos.Where(propInfo => Attribute.IsDefined(propInfo, typeof (RequiredAttribute)))
                .Select(propInfo => propInfo.Name)
                .ToList();

            var polymorphicType = PolymorphicTypeFor(type);
            var subTypeSpecs = polymorphicType.SubTypes
                .Select(subType => CreateSpecFor(subType.Type, true, complexTypeMappings))
                .Select(subTypeSpec => subTypeSpec.Ref)
                .ToList();

            return new ModelSpec
            {
                Id = UniqueIdFor(type),
                Type = "object",
                Properties = propSpecs,
                Required = required,
                SubTypes = subTypeSpecs,
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
                polymorphicType = baseType.SearchHierarchyFor(type);
                if (polymorphicType != null) return polymorphicType;
            }

            return new PolymorphicType(type);
        }
    }
}