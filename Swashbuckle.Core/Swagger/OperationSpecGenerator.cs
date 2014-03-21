using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;

namespace Swashbuckle.Core.Swagger
{
    public class OperationSpecGenerator
    {
        private readonly IEnumerable<IOperationSpecFilter> _operationFilters;
        private readonly ModelSpecGenerator _modelSpecGenerator;

        public OperationSpecGenerator(
            IDictionary<Type, ModelSpec> customTypeMappings,
            Dictionary<Type, IEnumerable<Type>> subTypesLookup,
            IEnumerable<IOperationSpecFilter> operationFilters)
        {
            _operationFilters = operationFilters;
            _modelSpecGenerator = new ModelSpecGenerator(customTypeMappings, subTypesLookup);
        }

        public OperationSpec ApiDescriptionToOperationSpec(ApiDescription apiDescription, Dictionary<string, ModelSpec> complexModels)
        {
            var apiPath = apiDescription.RelativePathSansQueryString();
            var paramSpecs = apiDescription.ParameterDescriptions
                .Select(paramDesc => CreateParameterSpec(paramDesc, apiPath, complexModels))
                .ToList();

            var operationSpec = new OperationSpec
            {
                Method = apiDescription.HttpMethod.Method,
                Nickname = apiDescription.Nickname(),
                Summary = apiDescription.Documentation,
                Parameters = paramSpecs,
                ResponseMessages = new List<ResponseMessageSpec>()
            };

            var responseType = apiDescription.ActualResponseType();
            if (responseType == null)
            {
                operationSpec.Type = "void";
            }
            else
            {
                IDictionary<string, ModelSpec> returnTypeComplexModels;
                var modelSpec = _modelSpecGenerator.TypeToModelSpec(responseType, out returnTypeComplexModels);

                complexModels.Merge(returnTypeComplexModels);

                if (modelSpec.Type == "object")
                {
                    operationSpec.Type = modelSpec.Id;
                }
                else
                {
                    operationSpec.Type = modelSpec.Type;
                    operationSpec.Format = modelSpec.Format;
                    operationSpec.Items = modelSpec.Items;
                    operationSpec.Enum = modelSpec.Enum;
                }
            }

            foreach (var filter in _operationFilters)
            {
                filter.Apply(operationSpec, apiDescription, _modelSpecGenerator, complexModels);
            }

            return operationSpec;
        }

        private ParameterSpec CreateParameterSpec(ApiParameterDescription apiParamDesc, string apiPath, Dictionary<string, ModelSpec> complexModels)
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

            var paramSpec = new ParameterSpec
            {
                ParamType = paramType,
                Name = apiParamDesc.Name,
                Description = apiParamDesc.Documentation,
                Required = !apiParamDesc.ParameterDescriptor.IsOptional
            };

            IDictionary<string, ModelSpec> parameterComplexModels;
            var modelSpec = _modelSpecGenerator.TypeToModelSpec(apiParamDesc.ParameterDescriptor.ParameterType, out parameterComplexModels);

            complexModels.Merge(parameterComplexModels);

            if (modelSpec.Type == "object")
            {
                paramSpec.Type = modelSpec.Id;
            }
            else
            {
                paramSpec.Type = modelSpec.Type;
                paramSpec.Format = modelSpec.Format;
                paramSpec.Items = modelSpec.Items;
                paramSpec.Enum = modelSpec.Enum;
            }

            return paramSpec;
        }
    }
}
