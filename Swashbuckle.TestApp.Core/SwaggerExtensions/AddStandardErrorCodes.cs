using System.Collections.Generic;
using System.Net;
using System.Web.Http.Description;
using Swashbuckle.Core.Swagger;

namespace Swashbuckle.TestApp.Core.SwaggerExtensions
{
    public class AddStandardErrorCodes : IOperationSpecFilter
    {
        public void Apply(OperationSpec operationSpec, Dictionary<string, ModelSpec> complexModels, ModelSpecGenerator modelSpecGenerator, ApiDescription apiDescription)
        {
            operationSpec.ResponseMessages.Add(new ResponseMessageSpec
            {
                Code = (int)HttpStatusCode.OK,
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