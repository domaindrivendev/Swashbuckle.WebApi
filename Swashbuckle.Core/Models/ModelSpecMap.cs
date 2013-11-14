using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swashbuckle.Core.Models
{
    public class ModelSpecMap
    {
        private readonly Dictionary<Type, ModelSpec> _mappings = new Dictionary<Type, ModelSpec>();

        private static readonly Dictionary<Type, ModelSpec> PrimitiveTypeMap =
            new Dictionary<Type, ModelSpec>
                {
                    {typeof(Int32), new ModelSpec{ Type = "integer", Format = "int32"}},
                    {typeof(UInt32), new ModelSpec{ Type = "integer", Format = "int32"}},
                    {typeof(Int64), new ModelSpec{ Type = "integer", Format = "int64"}},
                    {typeof(UInt64), new ModelSpec{ Type = "integer", Format = "int64"}},
                    {typeof(Single), new ModelSpec{ Type = "number", Format = "float"}},
                    {typeof(Double), new ModelSpec{ Type = "number", Format = "double"}},
                    {typeof(Decimal), new ModelSpec{ Type = "number", Format = "double"}},
                    {typeof(String), new ModelSpec{ Type = "string", Format = null}},
                    {typeof(Byte), new ModelSpec{ Type = "string", Format = "byte"}},
                    {typeof(Boolean), new ModelSpec{ Type = "boolean", Format = null}},
                    {typeof(DateTime), new ModelSpec{ Type = "string", Format = "date-time"}}
                };

        public ModelSpec FindOrCreateFor(Type type)
        {
            // Create mapping if it doesn't exist
            if (!_mappings.ContainsKey(type))
                _mappings[type] = CreateModelSpec(type);

            return _mappings[type];
        }

        public IEnumerable<ModelSpec> GetAll()
        {
            return _mappings.Values;
        }

        private ModelSpec CreateModelSpec(Type type)
        {
            // Primitives, incl. enums
            if (PrimitiveTypeMap.ContainsKey(type))
                return PrimitiveTypeMap[type];

            if (type.IsEnum)
                return new ModelSpec
                    {
                        Type = "string",
                        Enum = type.GetEnumNames()
                    };

            Type enumerableTypeArgument;
            if (type.IsEnumerable(out enumerableTypeArgument))
                return CreateContainerSpec(enumerableTypeArgument);

            Type nullableTypeArgument;
            if (type.IsNullable(out nullableTypeArgument))
                return FindOrCreateFor(nullableTypeArgument);

            return CreateComplexSpec(type);
        }

        private ModelSpec CreateContainerSpec(Type containedType)
        {
            var containedModelSpec = FindOrCreateFor(containedType);
            var modelSpec = new ModelSpec
            {
                Type = "array",
                Items = containedModelSpec
            };

            if (containedModelSpec.Type == "object")
                modelSpec.Items = new ModelSpec { Ref = containedModelSpec.Id };

            return modelSpec;
        }

        private ModelSpec CreateComplexSpec(Type type)
        {
            var modelSpec = new ModelSpec
            {
                Id = GetNameForComplexType(type),
                Type = "object",
                Properties = new Dictionary<string, ModelSpec>()
            };

            foreach (var propInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var propModelSpec = FindOrCreateFor(propInfo.PropertyType);

                if (propModelSpec.Type == "object")
                    propModelSpec = new ModelSpec { Ref = propModelSpec.Id };

                modelSpec.Properties.Add(propInfo.Name, propModelSpec);
            }

            return modelSpec;
        }

        private string GetNameForComplexType(Type type)
        {
            var typeName =type.Name.Split('.').First();
            
            if (type.IsGenericType)
            {
                var genericName = type.GetGenericArguments().First().Name.Split('.').First();
                typeName = string.Format("{0}-{1}", typeName, genericName);
            }
            return typeName;
        }
    }

}
