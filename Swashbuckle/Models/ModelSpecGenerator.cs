﻿using System;
using System.Collections.Generic;
﻿using System.ComponentModel.DataAnnotations;
﻿using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.Models
{
    public class ModelSpecGenerator
    {
        private static readonly Dictionary<Type, ModelSpec> PrimitiveMappings = new Dictionary<Type, ModelSpec>()
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
                {typeof (HttpResponseMessage), new ModelSpec{Id = "Object", Type="object"}},
                {typeof (JObject), new ModelSpec{Id = "Object", Type="object"}}
            };

        private readonly IDictionary<Type, ModelSpec> _customMappings;

        public ModelSpecGenerator(IDictionary<Type, ModelSpec> customMappings)
        {
            if (customMappings == null)
                throw new ArgumentNullException("customMappings");

            _customMappings = customMappings;
        }

        public ModelSpecGenerator()
            : this(new Dictionary<Type, ModelSpec>())
        { }

        public ModelSpec TypeToModelSpec(Type type, out IEnumerable<ModelSpec> referencedSpecs)
        {
            // Track progress in a dictionary, deferred type->spec generation is indicated with a null Value
            var referencedTypes = new Dictionary<Type, ModelSpec>();

            var rootSpec = CreateSpecFor(type, false, referencedTypes);

            while (referencedTypes.ContainsValue(null))
            {
                var referencedType = referencedTypes.First(kvp => kvp.Value == null).Key;
                var spec = CreateSpecFor(referencedType, false, referencedTypes);
                referencedTypes[referencedType] = spec;
            }

            referencedSpecs = referencedTypes.Select(kvp => kvp.Value);
            return rootSpec;
        }

        private ModelSpec CreateSpecFor(Type type, bool deferIfComplex, IDictionary<Type, ModelSpec> referencedTypes)
        {
            if (_customMappings.ContainsKey(type))
                return _customMappings[type];

            if (PrimitiveMappings.ContainsKey(type))
                return PrimitiveMappings[type];

            if (type.IsEnum)
                return new ModelSpec { Type = "string", Enum = type.GetEnumNames() };

            Type innerType;
            if (type.IsNullable(out innerType))
                return CreateSpecFor(innerType, deferIfComplex, referencedTypes);

            Type itemType;
            if (type.IsEnumerable(out itemType))
                return new ModelSpec { Type = "array", Items = CreateSpecFor(itemType, true, referencedTypes) };

            // Anthing else is complex

            if (deferIfComplex)
            {
                if (!referencedTypes.ContainsKey(type))
                    referencedTypes.Add(type, null);

                // Just return a reference for now
                return new ModelSpec { Ref = UniqueIdFor(type) };
            }

            return CreateComplexSpecFor(type, referencedTypes);
        }

        private ModelSpec CreateComplexSpecFor(Type type, IDictionary<Type, ModelSpec> referencedTypes)
        {
            var propInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var propSpecs = propInfos
                .ToDictionary(propInfo => propInfo.Name, propInfo => CreateSpecFor(propInfo.PropertyType, true, referencedTypes));

            var required = propInfos.Where(propInfo => Attribute.IsDefined(propInfo, typeof (RequiredAttribute)))
                .Select(propInfo => propInfo.Name)
                .ToList();

            return new ModelSpec
            {
                Id = UniqueIdFor(type),
                Type = "object",
                Properties = propSpecs,
                Required = required
            };
        }

        private static string UniqueIdFor(Type type)
        {
            if (type.IsGenericType)
            {
                var genericArguments = type.GetGenericArguments()
                    .Select(UniqueIdFor)
                    .ToArray();

                var builder = new StringBuilder(type.ShortName());

                return builder
                    .Replace(String.Format("`{0}", genericArguments.Count()), String.Empty)
                    .Append(String.Format("[{0}]", String.Join(",", genericArguments).TrimEnd(',')))
                    .ToString();
            }

            return type.ShortName();
        }
    }
}
