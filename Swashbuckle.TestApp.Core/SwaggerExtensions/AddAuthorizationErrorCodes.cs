using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Swashbuckle.Core.Swagger;

namespace Swashbuckle.TestApp.Core.SwaggerExtensions
{
    public class AddAuthorizationErrorCodes : IOperationSpecFilter
    {
        public void Apply(
            OperationSpec operationSpec,
            ApiDescription apiDescription,
            ModelSpecGenerator modelSpecGenerator,
            Dictionary<string, ModelSpec> complexModels)
        {
            if (apiDescription.ActionDescriptor.GetFilters().OfType<AuthorizeAttribute>().Any())
            {
                operationSpec.ResponseMessages.Add(new ResponseMessageSpec
                {
                    Code = (int)HttpStatusCode.Unauthorized,
                    Message = "Authentication required"
                });
            }
        }
    }
}