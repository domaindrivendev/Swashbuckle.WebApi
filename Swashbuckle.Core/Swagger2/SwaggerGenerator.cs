using System.Linq;
using System.Collections.Generic;
using System.Web.Http.Description;
using System;

namespace Swashbuckle.Swagger2
{
    public class SwaggerGenerator : ISwaggerProvider
    {
        private readonly IApiExplorer _apiExplorer;
        private readonly SwaggerGeneratorSettings _settings;
        private readonly SchemaRegistry _schemaRegistry;

        public SwaggerGenerator(
            IApiExplorer apiExplorer,
            SwaggerGeneratorSettings settings)
        {
            _apiExplorer = apiExplorer;
            _settings = settings;
            _schemaRegistry = new SchemaRegistry();
        }

        public SwaggerObject GetSwaggerFor(string apiVersion)
        {
            var paths = GetApplicableApiDescriptionsFor(apiVersion)
                .GroupBy(apiDesc => apiDesc.RelativePathSansQueryString())
                .ToDictionary(group => "/" + group.Key, group => CreatePathItem(group));

            return new SwaggerObject
            {
                info = _settings.Info,
                paths = paths
            };
        }

        private IEnumerable<ApiDescription> GetApplicableApiDescriptionsFor(string apiVersion)
        {
            return _apiExplorer.ApiDescriptions
                .Where(apiDesc => !_settings.IgnoreObsoleteActions || apiDesc.IsNotObsolete());
        }

        private PathItem CreatePathItem(IEnumerable<ApiDescription> apiDescriptions)
        {
            var pathItem = new PathItem();

            // Group further by http method
            var perMethodGrouping = apiDescriptions
                .GroupBy(apiDesc => apiDesc.HttpMethod.Method.ToLower());

            foreach (var group in perMethodGrouping)
            {
                var httpMethod = group.Key;

                if (group.Count() > 1)
                    throw new NotSupportedException(String.Format(
                        "Not supported by Swagger 2.0: Multiple operations with path '{0}' and method '{1}'",
                        "/" + group.First().RelativePathSansQueryString(),
                        httpMethod));

                var apiDescription = group.Single();
                switch (httpMethod)
                {
                    case "get":
                        pathItem.get = CreateOperation(apiDescription);
                        break;
                    case "put":
                        pathItem.put = CreateOperation(apiDescription);
                        break;
                    case "post":
                        pathItem.post = CreateOperation(apiDescription);
                        break;
                    case "delete":
                        pathItem.delete = CreateOperation(apiDescription);
                        break;
                    case "options":
                        pathItem.options = CreateOperation(apiDescription);
                        break;
                    case "head":
                        pathItem.head = CreateOperation(apiDescription);
                        break;
                    case "patch":
                        pathItem.patch = CreateOperation(apiDescription);
                        break;
                }
            }

            return pathItem;
        }

        private Operation CreateOperation(ApiDescription apiDescription)
        {
            var parameters = apiDescription.ParameterDescriptions
                .Select(paramDesc =>
                    {
                        var inPath = apiDescription.RelativePathSansQueryString().Contains("{" + paramDesc.Name + "}");
                        return CreateParameter(paramDesc, inPath);
                    });

            return new Operation
            {
                operationId = apiDescription.OperationId(),
                produces = apiDescription.Produces().ToList(),
                consumes = apiDescription.Consumes().ToList(),
                parameters = parameters.ToList()
            };
        }

        private Parameter CreateParameter(ApiParameterDescription paramDesc, bool inPath)
        {
            var @in = (inPath)
                ? "path"
                : (paramDesc.Source == ApiParameterSource.FromUri) ? "query" : "body";

            var parameter = new Parameter
            {
                name = paramDesc.Name,
                @in = @in
            };

            if (paramDesc.ParameterDescriptor == null)
            {
                parameter.type = "string";
                parameter.required = true;
                return parameter; 
            }

            parameter.required = !paramDesc.ParameterDescriptor.IsOptional;

            var schema = _schemaRegistry.FindOrRegister(paramDesc.ParameterDescriptor.ParameterType);
            if (parameter.@in == "body")
            {
                parameter.schema = schema;
            }
            else
            {
                parameter.type = schema.type;
            }

            return parameter;
        }
    }
}