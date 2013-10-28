using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.Models
{
    public class ModelSpecMap
    {
        private readonly Dictionary<Type, ModelSpec> _mappings = new Dictionary<Type, ModelSpec>()
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
                {typeof (HttpResponseMessage), new ModelSpec()},
                {typeof (JObject), new ModelSpec {Type = "string"}}
            };

        public ModelSpecMap() : this(null) {}

        public ModelSpecMap(IDictionary<Type, ModelSpec> customTypeMappings)
        {
            if (customTypeMappings != null)
                _mappings.MergeWith(customTypeMappings);
        }

        public ModelSpec FindOrCreateFor(Type type)
        {
            // Track any mappings that are currently being created to avoid infinite recursive loop
            var wip = new Stack<Type>();
            return FindOrCreateSpecFor(type, wip);
        }

        public IEnumerable<ModelSpec> GetAll()
        {
            return _mappings.Values;
        }

        private ModelSpec FindOrCreateSpecFor(Type type, Stack<Type> wip)
        {
            wip.Push(type);

            if (!_mappings.ContainsKey(type))
            {
                Type enumerableTypeArgument;
                Type nullableTypeArgument;

                if (type.IsEnum)
                {
                    _mappings.Add(type, CreateEnumSpecFor(type));
                }
                else if (type.IsEnumerable(out enumerableTypeArgument))
                {
                    _mappings.Add(type, CreateContainerSpecFor(enumerableTypeArgument, wip));
                }
                else if (type.IsNullable(out nullableTypeArgument))
                {
                    _mappings.Add(type, FindOrCreateSpecFor(nullableTypeArgument, wip));
                }
                else
                {
                    _mappings.Add(type, CreateComplexSpecFor(type, wip));  
                }
            }

            wip.Pop();

            return _mappings[type];
        }

        private ModelSpec CreateEnumSpecFor(Type type)
        {
            return new ModelSpec
                {
                    Type = "string",
                    Enum = type.GetEnumNames()
                };
        }

        private ModelSpec CreateContainerSpecFor(Type containedType, Stack<Type> wip)
        {
            if (wip.Contains(containedType) && !_mappings.ContainsKey(containedType))
            {
                // Safe to assume contained type is complex
                var id = GetIdentifierFor(containedType);
                return new ModelSpec
                    {
                        Type = "array",
                        Items = new ModelSpec {Ref = id}
                    };
            }

            var itemsSpec = FindOrCreateSpecFor(containedType, wip);
            var modelSpec = new ModelSpec
            {
                Type = "array",
                Items = itemsSpec
            };

            if (itemsSpec.Type == "object")
                modelSpec.Items = new ModelSpec { Ref = itemsSpec.Id };

            return modelSpec;
        }

        private ModelSpec CreateComplexSpecFor(Type type, Stack<Type> wip)
        {
            var modelSpec = new ModelSpec
            {
                Id = GetIdentifierFor(type),
                Type = "object",
                Properties = new Dictionary<string, ModelSpec>()
            };

            foreach (var propInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var propType = propInfo.PropertyType;
                if (wip.Contains(propType) && !_mappings.ContainsKey(propType))
                {
                    // Safe to assume contained type is complex
                    var id = GetIdentifierFor(propType);
                    modelSpec.Properties.Add(propInfo.Name, new ModelSpec { Ref = id });
                }
                else
                {
                    var propSpec = FindOrCreateSpecFor(propType, wip);

                    if (propSpec.Type == "object")
                        propSpec = new ModelSpec { Ref = propSpec.Id };

                    modelSpec.Properties.Add(propInfo.Name, propSpec);    
                }
            }

            return modelSpec;
        }

        private string GetIdentifierFor(Type type)
        {
            if (type.IsGenericType)
            {
                var genericArguments = type.GetGenericArguments()
                    .Select(GetIdentifierFor)
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
