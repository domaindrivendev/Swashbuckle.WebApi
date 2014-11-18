using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;
using System.Collections.Generic;

namespace Swashbuckle.SwaggerExtensions
{
    public class HandleParamsFromUri : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if(operation.parameters == null)
                return;

            HandleArrayParamsFromUri(operation);

            HandleObjectParamsFromUri(operation, schemaRegistry, apiDescription);
        }

        private static void HandleArrayParamsFromUri(Operation operation)
        {
            var arrayParamsFromUri = operation.parameters
                .Where(param => param.@in == "query" && param.type == "array")
                .ToArray();

            foreach (var param in arrayParamsFromUri)
            {
                param.collectionFormat = "multi";
            }
        }

        private void HandleObjectParamsFromUri(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var objectParamsFromUri = operation.parameters
                .Where(param => param.@in == "query" && param.type == null)
                .ToArray();

            foreach (var param in objectParamsFromUri)
            {
                var paramType = apiDescription.ParameterDescriptions
                    .Single(paramDesc => paramDesc.Name == param.name)
                    .ParameterDescriptor.ParameterType;

                var @ref = paramType.FriendlyId();
                var schema = schemaRegistry.Definitions[@ref];

                operation.parameters.Remove(param);
                AddQueryParameterPerSchemaProperty(operation.parameters, schema);
            }
        }

        private void AddQueryParameterPerSchemaProperty(IList<Parameter> parameters, Schema schema)
        {
            foreach (var entry in schema.properties)
            {
                var param = new Parameter
                {
                    name = entry.Key.ToLowerInvariant(),
                    @in = "query",
                    required = schema.required != null && schema.required.Contains(entry.Key)
                };
                param.PopulateFrom(entry.Value);
                parameters.Add(param);
            }
        }
    }
}