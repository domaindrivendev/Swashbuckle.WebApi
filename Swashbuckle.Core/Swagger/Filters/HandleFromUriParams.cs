using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;
using System.Collections.Generic;

namespace Swashbuckle.Swagger.Filters
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

                var refSchema = schemaRegistry.GetOrRegister(paramType);
                var schema = schemaRegistry.Definitions[refSchema.@ref.Replace("#/definitions/", "")];

                operation.parameters.Remove(param);
                MapSchemaPropertiesToQueryParams(schema, operation.parameters);
            }
        }

        private void MapSchemaPropertiesToQueryParams(Schema schema, IList<Parameter> parameters)
        {
            // TODO: Support nested properties with dot syntax
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