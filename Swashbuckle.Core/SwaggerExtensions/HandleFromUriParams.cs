using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;
using System.Collections.Generic;

namespace Swashbuckle.SwaggerExtensions
{
    public class HandleFromUriParams : IOperationFilter
    {
        public void Apply(Operation operation, DataTypeRegistry dataTypeRegistry, ApiDescription apiDescription)
        {
            if (operation.Parameters == null)
                return;

            HandleFromUriObjectParams(operation, dataTypeRegistry, apiDescription);
        }

        private void HandleFromUriObjectParams(Operation operation, DataTypeRegistry dataTypeRegistry, ApiDescription apiDescription)
        {
            var models = dataTypeRegistry.GetModels();

            var fromUriObjectParams = operation.Parameters
                .Where(param => param.ParamType == "query" && models.ContainsKey(param.Type))
                .ToArray();

            foreach (var objectParam in fromUriObjectParams)
            {
                var type = apiDescription.ParameterDescriptions
                    .Single(paramDesc => paramDesc.Name == objectParam.Name)
                    .ParameterDescriptor.ParameterType;

                var model = models[objectParam.Type];

                ExtractAndAddQueryParams(model, "", objectParam.Required, models, operation.Parameters);
                operation.Parameters.Remove(objectParam);
            }
        }

        private void ExtractAndAddQueryParams(
            DataType sourceModel,
            string sourceQualifier,
            bool sourceRequired,
            IDictionary<string, DataType> models,
            IList<Parameter> operationParams)
        {
            foreach (var entry in sourceModel.Properties)
            {
                var propertyDataType = entry.Value;
                var required = sourceRequired
                    && sourceModel.Required != null && sourceModel.Required.Contains(entry.Key);

                if (propertyDataType.Ref != null)
                {
                    var model = models[propertyDataType.Ref];
                    ExtractAndAddQueryParams(
                        model,
                        sourceQualifier + entry.Key.ToLowerInvariant() + ".",
                        required,
                        models,
                        operationParams);
                }
                else
                {
                    var param = new Parameter
                    {
                        Name = sourceQualifier + entry.Key.ToLowerInvariant(),
                        Description = propertyDataType.Description,
                        Type = propertyDataType.Type,
                        Format = propertyDataType.Format,
                        Items = propertyDataType.Items,
                        UniqueItems = propertyDataType.UniqueItems,
                        ParamType = "query",
                        Enum = propertyDataType.Enum,
                        Required = required
                    };
                    operationParams.Add(param);
                }
            }
        }
    }
}