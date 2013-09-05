using System.Web.Http.Description;
using Swashbuckle.Models;

namespace Swashbuckle.TestApp.SwaggerFilters
{
    public class AddErrorCodeFilter : IOperationSpecFilter
    {
        private readonly int _code;
        private readonly string _reason;

        public AddErrorCodeFilter(int code, string reason)
        {
            _code = code;
            _reason = reason;
        }

        public void Apply(ApiDescription apiDescription, ApiOperationSpec operationSpec)
        {
            operationSpec.responseMessages.Add(new ApiResponseMessageSpec {code = _code, message = _reason});
        }
    }
}