//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using Newtonsoft.Json.Schema;
//
//namespace Swashbuckle.Models
//{
//    public class ModelSpecsBuilder
//    {
//        private readonly JsonSchemaGenerator _jsonSchemaGenerator;
//        private readonly List<Type> _apiTypes;
//
//        public ModelSpecsBuilder()
//        {}
//
//        public ModelSpecsBuilder(JsonSchemaGenerator jsonSchemaGenerator)
//        {
//            _jsonSchemaGenerator = jsonSchemaGenerator;
//            _apiTypes = new List<Type>();
//        }
//
//        public ModelSpecsBuilder AddType(Type type)
//        {
//            if (!_apiTypes.Contains(type))
//                _apiTypes.Add(type);
//            return this;
//        }
//
//        public Dictionary<string, JsonSchemaSpec> Build()
//        {
//            var modelSpecs = new Dictionary<string, JsonSchemaSpec>();
//            return modelSpecs;
//        }
////
////        private void AddToModelSpecs(IEnumerable<Type> apiTypes, Dictionary<string, JsonSchemaSpec> modelSpecs)
////        {
////            foreach (var type in apiTypes)
////            {
////                var swaggerType = type.ToSwaggerType();
////                var category = swaggerType.Category;
////
////                if (category == SwaggerTypeCategory.Unknown || category == SwaggerTypeCategory.Primitive) continue;
////
////                if (modelSpecs.ContainsKey(swaggerType.Name)) continue;
////
////                var relatedTypes = new List<Type>();
////                if (category == SwaggerTypeCategory.Container)
////                {
////                    //relatedTypes.Add(swaggerType.ContainedType);
////                }
////                else
////                {
////                    var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
////                    var propertySpecs = properties
////                        .ToDictionary(pi => pi.Name, PropertyInfoToPropertySpec);
////
////                    modelSpecs.Add(swaggerType.Name, new ModelSpec {id = swaggerType.Name, properties = propertySpecs});
////
////                    relatedTypes.AddRange(properties.Select(p => p.PropertyType));
////                }
////
////                AddToModelSpecs(relatedTypes, modelSpecs);
////            }
////        }
////
////        private ModelPropertySpec PropertyInfoToPropertySpec(PropertyInfo popertyInfo)
////        {
////            var swaggerType = popertyInfo.PropertyType.ToSwaggerType();
////            return new ModelPropertySpec
////                {
////                    type = swaggerType.Name,
////                    format = swaggerType.Format,
////                    @enum = swaggerType.Enum
////                };
////        }
//    }
//}