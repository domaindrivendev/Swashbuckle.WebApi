using System.Collections.Generic;
using System.Web.Http.Description;

namespace Swashbuckle.Core.Swagger
{
    public interface IOperationSpecFilter
    {
        void Apply(OperationSpec operationSpec, Dictionary<string, ModelSpec> complexModels, ModelSpecGenerator modelSpecGenerator, ApiDescription apiDescription);
    }
}