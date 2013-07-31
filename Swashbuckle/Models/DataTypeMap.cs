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
        private readonly Dictionary<string, ModelSpec> _modelSpecs;

        public DataTypeMap(IEnumerable<Type> types)
        {
            _modelSpecs = new Dictionary<string, ModelSpec>();
            AddToModelSpecs(types);
        }

        public string DataTypeFor(Type type)
        {
            return type.ToSwaggerType();
        }

        private string DataTypeFor(Type type, out TypeCategory category, out Type containedType)
        {
            return type.ToSwaggerType(out category, out containedType);
        }

        private void AddToModelSpecs(IEnumerable<Type> apiTypes)
        {
            foreach (var type in apiTypes)
            {
                TypeCategory category;
                Type containedType;
                var dataType = DataTypeFor(type, out category, out containedType);

                if (_modelSpecs.ContainsKey(dataType)) continue;

                if (category == TypeCategory.Unkown || category == TypeCategory.Primitive) continue;

                var relatedTypes = new List<Type>();
                if (category == TypeCategory.Container)
                {
                    relatedTypes.Add(containedType);
                }
                else
                {
                    var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    var propertySpecs = properties
                        .ToDictionary(pi => pi.Name, pi => new ModelPropertySpec
                        {
                            type = DataTypeFor(pi.PropertyType),
                            required = true,
                            allowableValues = AllowableValuesFor(pi.PropertyType)
                        });
                    _modelSpecs.Add(dataType, new ModelSpec { id = dataType, properties = propertySpecs });

                    relatedTypes.AddRange(properties.Select(p => p.PropertyType));    
                }

                AddToModelSpecs(relatedTypes);
            }
        }

        public AllowableValuesSpec AllowableValuesFor(Type type)
        {
            Type innerType;
            if (type.IsNullableType(out innerType))
                return AllowableValuesFor(innerType);
            
            if (!type.IsEnum)
                return null;

            return new EnumeratedValuesSpec
            {
                values = type.GetEnumNames()
            };
        }

        public Dictionary<string, ModelSpec> ModelSpecs()
        {
            return _modelSpecs;
        }
    }
}
