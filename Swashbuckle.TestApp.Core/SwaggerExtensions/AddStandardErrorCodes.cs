using System.Collections.Generic;
using System.Net;
using System.Web.Http.Description;
using Swashbuckle.Core.Swagger;

namespace Swashbuckle.TestApp.Core.SwaggerExtensions
{
    public class AddStandardErrorCodes : IOperationFilter
    {
        public void Apply(Operation operation, Dictionary<string, DataType> complexModels, DataTypeGenerator dataTypeGenerator, ApiDescription apiDescription)
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