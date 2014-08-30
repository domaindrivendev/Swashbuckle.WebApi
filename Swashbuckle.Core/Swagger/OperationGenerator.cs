using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;

namespace Swashbuckle.Swagger
{
    public class OperationGenerator
    {
        private readonly TypeSystem _typeSystem;
        private readonly IEnumerable<IOperationFilter> _operationFilters;

        public OperationGenerator(TypeSystem typeSystem, IEnumerable<IOperationFilter> operationFilters)
        {
            _typeSystem = typeSystem;
            _operationFilters = operationFilters;
        }

        public Operation ApiDescriptionToOperation(ApiDescription apiDescription)
        {
            var apiPath = apiDescription.RelativePathSansQueryString();
            var parameters = apiDescription.ParameterDescriptions
                .Select(paramDesc => CreateParameter(paramDesc, apiPath))
                .ToList();

            var operation = new Operation
            {
                Method = apiDescription.HttpMethod.Method,
                Nickname = apiDescription.Nickname(),
                Summary = apiDescription.Documentation ?? "",
                Parameters = parameters,
                ResponseMessages = new List<ResponseMessage>(),
                Produces = apiDescription.SupportedResponseFormatters.SelectMany(d => d.SupportedMediaTypes.Select(t => t.MediaType)).ToList(),
                Consumes = apiDescription.SupportedRequestBodyFormatters.SelectMany(d => d.SupportedMediaTypes.Select(t => t.MediaType)).ToList()
            };

            var responseType = apiDescription.ActualResponseType();
            if (responseType == null)
            {
                operation.Type = "void";
            }
            else
            {
                var dataType = _typeSystem.GetDataTypeFor(responseType);
                if (dataType.Ref != null)
                    operation.Type = dataType.Ref;
                else
                    operation.CopyValuesFrom(dataType);
            }

            foreach (var filter in _operationFilters)
            {
                filter.Apply(operation, _typeSystem, apiDescription);
            }

            return operation;
        }

        private Parameter CreateParameter(ApiParameterDescription apiParamDesc, string apiPath)
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

            if (apiParamDesc.ParameterDescriptor == null)
            {
                return new Parameter { ParamType = paramType, Name = apiParamDesc.Name, Required = true, Type = "string" };
            }

            var parameter = new Parameter
            {
                ParamType = paramType,
                Name = apiParamDesc.Name,
                Description = apiParamDesc.Documentation,
                Required = !apiParamDesc.ParameterDescriptor.IsOptional
            };

            var dataType = _typeSystem.GetDataTypeFor(apiParamDesc.ParameterDescriptor.ParameterType);
            if (dataType.Ref != null)
                parameter.Type = dataType.Ref;
            else
                parameter.CopyValuesFrom(dataType);

            return parameter;
        }
    }
}
