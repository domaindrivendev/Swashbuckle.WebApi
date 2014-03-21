using System.Collections.Generic;
using System.Web.Http.Description;

namespace Swashbuckle.Core.Swagger
{
    public interface IOperationSpecFilter
    {
        void Apply(
            OperationSpec operationSpec,
            ApiDescription apiDescription,
            ModelSpecGenerator modelSpecGenerator,
            Dictionary<string, ModelSpec> complexModels);
    }
}