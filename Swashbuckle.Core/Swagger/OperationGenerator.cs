using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;

namespace Swashbuckle.Core.Swagger
{
    public class OperationGenerator
    {
        private readonly IEnumerable<IOperationFilter> _operationFilters;
        private readonly DataTypeGenerator _dataTypeGenerator;

        public OperationGenerator(IEnumerable<IOperationFilter> operationFilters, DataTypeGenerator dataTypeGenerator)
        {
            _operationFilters = operationFilters;
            _dataTypeGenerator = dataTypeGenerator;
        }

        public Operation ApiDescriptionToOperation(ApiDescription apiDescription, Dictionary<string, DataType> models)
        {
            var apiPath = apiDescription.RelativePathSansQueryString();
            var parameters = apiDescription.ParameterDescriptions
                .Select(paramDesc => CreateParameter(paramDesc, apiPath, models))
                .ToList();

            var operation = new Operation
            {
                Method = apiDescription.HttpMethod.Method,
                Nickname = apiDescription.Nickname(),
                Summary = apiDescription.Documentation,
                Parameters = parameters,
                ResponseMessages = new List<ResponseMessage>()
            };

            var responseType = apiDescription.ActualResponseType();
            if (responseType == null)
            {
                operation.Type = "void";
            }
            else
            {
                IDictionary<string, DataType> modelsForResponseType;
                var dataType = _dataTypeGenerator.TypeToDataType(responseType, out modelsForResponseType);

                models.Merge(modelsForResponseType);

                if (dataType.Type == "object")
                {
                    operation.Type = dataType.Id;
                }
                else
                {
                    operation.Type = dataType.Type;
                    operation.Format = dataType.Format;
                    operation.Items = dataType.Items;
                    operation.Enum = dataType.Enum;
                }
            }

            foreach (var filter in _operationFilters)
            {
                filter.Apply(operation, models, _dataTypeGenerator, apiDescription);
            }

            return operation;
        }

        private Parameter CreateParameter(ApiParameterDescription apiParamDesc, string apiPath, Dictionary<string, DataType> models)
        {
            var paramType = "";
            switch (apiParamDesc.Source)
            {
                case ApiParameterSource.FromBody:
                    paramType = "body";
                    break;
                case ApiParameterSource.FromUri:
                    paramType = apiPath.Contains(String.Format("{{{0}}}", apiParamDesc.Name)) ? "path" : "query";
                    break;
            }

            var parameter = new Parameter
            {
                ParamType = paramType,
                Name = apiParamDesc.Name,
                Description = apiParamDesc.Documentation,
                Required = !apiParamDesc.ParameterDescriptor.IsOptional
            };

            IDictionary<string, DataType> modelsForParameter;
            var dataType = _dataTypeGenerator.TypeToDataType(apiParamDesc.ParameterDescriptor.ParameterType, out modelsForParameter);

            models.Merge(modelsForParameter);

            if (dataType.Type == "object")
            {
                parameter.Type = dataType.Id;
            }
            else
            {
                parameter.Type = dataType.Type;
                parameter.Format = dataType.Format;
                parameter.Items = dataType.Items;
                parameter.Enum = dataType.Enum;
            }

            return parameter;
        }
    }
}
