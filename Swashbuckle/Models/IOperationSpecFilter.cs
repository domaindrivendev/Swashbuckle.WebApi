using System.Web.Http.Description;

namespace Swashbuckle.Models
{
    public interface IOperationSpecFilter
    {
        void UpdateSpec(ApiDescription apiDescription, ApiOperationSpec operationSpec);
    }
}