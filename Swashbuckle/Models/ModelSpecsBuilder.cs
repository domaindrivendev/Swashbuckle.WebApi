using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swashbuckle.Models
{
    public class ModelSpecsBuilder
    {
        private readonly List<Type> _apiTypes;

        public ModelSpecsBuilder()
        {
            _apiTypes = new List<Type>();
        }

        public ModelSpecsBuilder AddType(Type type)
        {
            if (!_apiTypes.Contains(type))
                _apiTypes.Add(type);
            return this;
        }

        public Dictionary<string, ModelSpec> Build()
        {
            var modelSpecs = new Dictionary<string, ModelSpec>();
            AddToModelSpecs(_apiTypes, modelSpecs);

            return modelSpecs;
        }

        private void AddToModelSpecs(IEnumerable<Type> apiTypes, Dictionary<string, ModelSpec> modelSpecs)
        {
            foreach (var type in apiTypes)
            {
                TypeCategory category;
                Type containedType;
                var dataType = type.ToSwaggerType(out category, out containedType);

                if (category == TypeCategory.Unkown || category == TypeCategory.Primitive) continue;

                if (modelSpecs.ContainsKey(dataType)) continue;

                var relatedTypes = new List<Type>();
                if (category == TypeCategory.Container)
                {
                    relatedTypes.Add(containedType);
                }
                else
                {
                    var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    var propertySpecs = properties
                        .ToDictionary(pi => pi.Name, pi => pi.PropertyType.ToModelPropertySpec());

                    modelSpecs.Add(dataType, new ModelSpec {id = dataType, properties = propertySpecs});

                    relatedTypes.AddRange(properties.Select(p => p.PropertyType));
                }

                AddToModelSpecs(relatedTypes, modelSpecs);
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
    }
}