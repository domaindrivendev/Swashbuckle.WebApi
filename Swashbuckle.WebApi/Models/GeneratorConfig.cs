using System.Collections.Generic;
using System.Web.Http.Description;

namespace Swashbuckle.WebApi.Models
{
    public interface IOperationSpecFilter
    {
        void UpdateSpec(ApiDescription apiDescription, ApiOperationSpec operationSpec);
    }

    public class GeneratorConfig
    {
        public GeneratorConfig()
        {
            Filters = new List<IOperationSpecFilter>();
        }

        public IList<IOperationSpecFilter> Filters { get; private set; }
    }
}