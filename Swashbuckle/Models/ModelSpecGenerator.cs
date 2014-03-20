﻿using System;
using System.Collections.Generic;
﻿using System.ComponentModel.DataAnnotations;
﻿using System.Dynamic;
﻿using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.Models
{
    public class ModelSpecGenerator
    {
        private static readonly Dictionary<Type, ModelSpec> StaticMappings = new Dictionary<Type, ModelSpec>()
            {
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
                {typeof (Object), new ModelSpec{Id = "Object", Type="object", Required = new List<string>()}},
                {typeof (ExpandoObject), new ModelSpec{Id = "Object", Type="object", Required = new List<string>()}},
                {typeof (JObject), new ModelSpec{Id = "Object", Type="object", Required = new List<string>()}},
                {typeof (HttpResponseMessage), new ModelSpec{Id = "Object", Type="object", Required = new List<string>()}}
            };

        private readonly IDictionary<Type, ModelSpec> _customMappings;
        private readonly Dictionary<Type, IEnumerable<Type>> _subTypesLookup;
        private readonly IEnumerable<IModelFilter> _modelFilters;

        public ModelSpecGenerator(IDictionary<Type, ModelSpec> customMappings,
            Dictionary<Type, IEnumerable<Type>> subTypesLookup,
            IEnumerable<IModelFilter> modelFilters)
        {
            if (customMappings == null)
                throw new ArgumentNullException("customMappings");

            _customMappings = customMappings;
            _subTypesLookup = subTypesLookup;
            _modelFilters = modelFilters;
        }

        public ModelSpec TypeToModelSpec(Type type, out IEnumerable<ModelSpec> complexSpecs)
        {
            // Track progress, any complex types with a spec that is pending generation will have a null entry
            var complexTypes = new Dictionary<Type, ModelSpec>();

            var rootSpec = CreateSpecFor(type, false, complexTypes);

            while (complexTypes.ContainsValue(null))
            {
                var complexType = complexTypes.First(kvp => kvp.Value == null).Key;
                var spec = CreateSpecFor(complexType, false, complexTypes);
                complexTypes[complexType] = spec;
            }

            complexSpecs = complexTypes.Select(kvp => kvp.Value)
                .Union(rootSpec.Type == "object" ? new[] {rootSpec} : new ModelSpec[]{});

            return rootSpec;
        }

        private ModelSpec CreateSpecFor(Type type, bool deferIfComplex, IDictionary<Type, ModelSpec> complexTypes)
        {
            if (_customMappings.ContainsKey(type))
                return _customMappings[type];

            if (StaticMappings.ContainsKey(type))
                return StaticMappings[type];

            if (type.IsEnum)
                return new ModelSpec { Type = "string", Enum = type.GetEnumNames() };

            Type innerType;
            if (type.IsNullable(out innerType))
                return CreateSpecFor(innerType, deferIfComplex, complexTypes);

            Type itemType;
            if (type.IsEnumerable(out itemType))
                return new ModelSpec { Type = "array", Items = CreateSpecFor(itemType, true, complexTypes) };

            // Anthing else is complex

            if (deferIfComplex)
            {
                if (!complexTypes.ContainsKey(type))
                    complexTypes.Add(type, null);

                // Just return a reference for now
                return new ModelSpec { Ref = UniqueIdFor(type) };
            }

            return CreateComplexSpecFor(type, complexTypes);
        }

        private ModelSpec CreateComplexSpecFor(Type type, IDictionary<Type, ModelSpec> complexTypes)
        {
            var propInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            //Clone ModelSpec for property so that alterations to the property (such as description) do not affect the originating ModelSpec
            var propSpecs = propInfos
                .ToDictionary(propInfo => propInfo.Name, propInfo => CreateSpecFor(propInfo.PropertyType, true, complexTypes).Clone());

            var required = propInfos.Where(propInfo => Attribute.IsDefined(propInfo, typeof (RequiredAttribute)))
                .Select(propInfo => propInfo.Name)
                .ToList();

            var subTypes = _subTypesLookup.ContainsKey(type)
                ? _subTypesLookup[type]
                : new Type[]{};

            var subTypeSpecs = subTypes
                .Select(subType => CreateSpecFor(subType, true, complexTypes))
                .Select(subTypeSpec => subTypeSpec.Ref)
                .ToList();

            var modelSpec = new ModelSpec
            {
                Id = UniqueIdFor(type),
                Type = "object",
                Properties = propSpecs,
                Required = required,
                SubTypes = subTypeSpecs
            };
            
            //Apply model filters
            foreach (var filter in _modelFilters)
            {
                filter.Apply(type, modelSpec);
            }

            return modelSpec;
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
