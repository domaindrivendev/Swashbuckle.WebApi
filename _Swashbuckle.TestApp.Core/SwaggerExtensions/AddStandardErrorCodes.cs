using System.Collections.Generic;
using System.Net;
using System.Web.Http.Description;
using Swashbuckle.Core.Swagger;

namespace Swashbuckle.TestApp.Core.SwaggerExtensions
{
    public class AddStandardErrorCodes : IOperationSpecFilter
    {
        public void Apply(
            OperationSpec operationSpec,
            ApiDescription apiDescription,
            ModelSpecGenerator modelSpecGenerator,
            Dictionary<string, ModelSpec> complexModels)
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