using System.Net;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Swashbuckle.Dummy.SwaggerExtensions
{
    public class AddStandardResponseCodes : IOperationFilter
    {
        public void Apply(Operation operation, DataTypeRegistry dataTypeRegistry, ApiDescription apiDescription)
        {
            operation.ResponseMessages.Add(new ResponseMessage
            {
                Code = (int)HttpStatusCode.OK,
                Message = "It's all good!"
            });

            operation.ResponseMessages.Add(new ResponseMessage
            {
                Code = (int)HttpStatusCode.InternalServerError,
                Message = "Somethings up!"
            });
        }
    }
}