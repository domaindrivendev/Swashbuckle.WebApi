using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace Swashbuckle.Models
{
    public class ModelMap
    {
        private static readonly StringDictionary PrimitiveTypeMap = new StringDictionary {
            {"Byte", "byte"},
            {"Boolean", "boolean"},
            {"Int32", "int"},
            {"Int64", "long"},
            {"Single", "float"},
            {"Double", "double"},
            {"Decimal", "double"},
            {"String", "string"},
            {"DateTime", "date"}
        };

        private static readonly StringDictionary ContainerTypeMap = new StringDictionary {
            {"IEnumerable`1", "List"},
        };

        private readonly Dictionary<string, ModelSpec> _modelSpecs;

        public ModelMap(IEnumerable<Type> uniqueTypes)
        {
            _modelSpecs = new Dictionary<string, ModelSpec>();
            AddToModelSpecs(uniqueTypes);
        }

        public string DataTypeFor(Type type)
        {
            var typeName = type.Name;
            if (PrimitiveTypeMap.ContainsKey(typeName))
                return PrimitiveTypeMap[typeName];

            if (ContainerTypeMap.ContainsKey(typeName))
                return String.Format("{0}[{1}]", ContainerTypeMap[typeName], DataTypeFor(GetContainedType(type)));

            return type.Name;
        }

        private Type GetContainedType(Type type)
        {
            return type.GetGenericArguments().First();
        }

        private void AddToModelSpecs(IEnumerable<Type> apiTypes)
        {
            foreach (var type in apiTypes)
            {
                var typeName = type.Name;
                if (PrimitiveTypeMap.ContainsKey(typeName)) continue;

                var dataType = DataTypeFor(type);
                if (_modelSpecs.ContainsKey(dataType)) continue;

                var relatedTypes = new List<Type>();
                if (ContainerTypeMap.ContainsKey(typeName))
                {
                    var itemType = GetContainedType(type);
                    relatedTypes.Add(itemType);
                }
                else
                {
                    var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    var propertySpecs = properties
                        .ToDictionary(pi => pi.Name, pi => new ModelPropertySpec { type = DataTypeFor(pi.PropertyType), required = true });
                    _modelSpecs.Add(dataType, new ModelSpec { id = dataType, properties = propertySpecs });

                    relatedTypes.AddRange(properties.Select(p => p.PropertyType));    
                }

                AddToModelSpecs(relatedTypes);
            }
        }

        public Dictionary<string, ModelSpec> ModelSpecs()
        {
            return _modelSpecs;
        }
    }
}
