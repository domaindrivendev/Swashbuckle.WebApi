using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Swashbuckle.Models;

namespace Swashbuckle.TestApp.Api.SwaggerExtensions
{
    public class AddAuthorizationErrorCodes : IOperationSpecFilter
    {
        public void Apply(ApiDescription apiDescription, OperationSpec operationSpec)
        {
            if (apiDescription.ActionDescriptor.GetFilters().OfType<AuthorizeAttribute>().Any())
            {
                operationSpec.ResponseMessages.Add(new ResponseMessageSpec
                    {
                        Code = (int) HttpStatusCode.Unauthorized,
                        Message = "Authentication required"
                    });
            }
        }
    }
}