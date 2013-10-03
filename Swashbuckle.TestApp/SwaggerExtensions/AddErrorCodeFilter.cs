using System.Web.Http.Description;
using Swashbuckle.Models;

namespace Swashbuckle.TestApp.SwaggerExtensions
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

        public void Apply(ApiDescription apiDescription, OperationSpec operationSpec)
        {
            operationSpec.ResponseMessages.Add(new ResponseMessageSpec {Code = _code, Message = _reason});
        }
    }
}