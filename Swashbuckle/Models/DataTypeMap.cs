using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace Swashbuckle.Models
{
    public class DataTypeMap
    {
        private enum Category
        {
            Unkown,
            Primitive,
            Container,
            Complex
        }

        private readonly Dictionary<string, ModelSpec> _modelSpecs;

        public DataTypeMap(IEnumerable<Type> types)
        {
            _modelSpecs = new Dictionary<string, ModelSpec>();
            AddToModelSpecs(types);
        }

        public string DataTypeFor(Type type)
        {
            Category category;
            Type containedType;
            return DataTypeFor(type, out category, out containedType);
        }

        private string DataTypeFor(Type type, out Category category, out Type containedType)
        {
            if (type == null)
            {
                category = Category.Unkown;
                containedType = null;
                return null;
            }

            var primitiveTypeMap = new StringDictionary {
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

            if (primitiveTypeMap.ContainsKey(type.Name))
            {
                category = Category.Primitive;
                containedType = null;
                return primitiveTypeMap[type.Name];    
            }

            var enumerable = type.AsGenericType(typeof (IEnumerable<>));
            if(enumerable != null)
            {
                category = Category.Container;
                containedType = enumerable.GetGenericArguments().First();
                return String.Format("List[{0}]", DataTypeFor(containedType));
            }

            category = Category.Complex;
            containedType = null;
            return type.Name;
        }

        private void AddToModelSpecs(IEnumerable<Type> apiTypes)
        {
            foreach (var type in apiTypes)
            {
                Category category;
                Type containedType;
                var dataType = DataTypeFor(type, out category, out containedType);

                if (_modelSpecs.ContainsKey(dataType)) continue;

                if (category == Category.Unkown || category == Category.Primitive) continue;

                var relatedTypes = new List<Type>();
                if (category == Category.Container)
                {
                    relatedTypes.Add(containedType);
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
