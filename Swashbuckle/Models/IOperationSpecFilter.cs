using System.Web.Http.Description;

namespace Swashbuckle.Area.Models
{
    public interface IOperationSpecFilter
    {
        void UpdateSpec(ApiDescription apiDescription, ApiOperationSpec operationSpec);
    }
}