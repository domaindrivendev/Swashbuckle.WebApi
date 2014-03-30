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
        public void Apply(OperationSpec operationSpec, Dictionary<string, ModelSpec> complexModels, ModelSpecGenerator modelSpecGenerator, ApiDescription apiDescription)
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