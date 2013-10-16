using System.Net;
using System.Web.Http.Description;
using Swashbuckle.Models;

namespace Swashbuckle.TestApp.Api.SwaggerExtensions
{
    public class AddStandardErrorCodes : IOperationSpecFilter
    {
        public void Apply(ApiDescription apiDescription, OperationSpec operationSpec)
        {
            operationSpec.ResponseMessages.Add(new ResponseMessageSpec
                {
                    Code = (int) HttpStatusCode.OK,
                    Message = "It's all good!"
                });

            operationSpec.ResponseMessages.Add(new ResponseMessageSpec
            {
                Code = (int)HttpStatusCode.InternalServerError,
                Message = "Somethings up!"
            });
        }
    }
}