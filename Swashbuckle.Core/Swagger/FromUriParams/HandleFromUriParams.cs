using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;
using System.Collections.Generic;

namespace Swashbuckle.Swagger.FromUriParams
{
    public class HandleFromUriParams : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if(operation.parameters == null)
                return;

            HandleFromUriArrayParams(operation);
            HandleFromUriObjectParams(operation, schemaRegistry, apiDescription);
        }

        private static void HandleFromUriArrayParams(Operation operation)
        {
            var fromUriArrayParams = operation.parameters
                .Where(param => param.@in == "query" && param.type == "array")
                .ToArray();

            foreach (var param in fromUriArrayParams)
            {
                param.collectionFormat = "multi";
            }
        }

        private void HandleFromUriObjectParams(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var fromUriObjectParams = operation.parameters
                .Where(param => param.@in == "query" && param.type == null)
                .ToArray();

            foreach (var objectParam in fromUriObjectParams)
            {
                var type = apiDescription.ParameterDescriptions
                    .Single(paramDesc => paramDesc.Name == objectParam.name)
                    .ParameterDescriptor.ParameterType;

                var refSchema = schemaRegistry.GetOrRegister(type);
                var schema = schemaRegistry.Definitions[refSchema.@ref.Replace("#/definitions/", "")];

                ExtractAndAddQueryParams(schema, "", objectParam.required, schemaRegistry, operation.parameters);
                operation.parameters.Remove(objectParam);
            }
        }

        private void ExtractAndAddQueryParams(
            Schema sourceSchema,
            string sourceQualifier,
            bool sourceRequired,
            SchemaRegistry schemaRegistry,
            IList<Parameter> operationParams)
        {
            foreach (var entry in sourceSchema.properties)
            {
                var propertySchema = entry.Value;
                var required = sourceRequired
                    && sourceSchema.required != null && sourceSchema.required.Contains(entry.Key); 

                if (propertySchema.@ref != null)
                {
                    var schema = schemaRegistry.Definitions[propertySchema.@ref.Replace("#/definitions/", "")];
                    ExtractAndAddQueryParams(
                        schema,
                        sourceQualifier + entry.Key.ToLowerInvariant() + ".",
                        required,
                        schemaRegistry,
                        operationParams);
                }
                else
                {
                    var param = new Parameter
                    {
                        name =  sourceQualifier + entry.Key.ToLowerInvariant(),
                        @in = "query",
                        required = required
                    };
                    param.PopulateFrom(entry.Value);
                    operationParams.Add(param);
                }
            }
        }
    }
}