using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Swashbuckle.Dummy.SwaggerExtensions
{
    public class AddAuthResponseCodes : IOperationFilter
    {
        public void Apply(Operation operation, TypeSystem typeSystem, ApiDescription apiDescription)
        {
            if (apiDescription.ActionDescriptor.GetFilters().OfType<AuthorizeAttribute>().Any())
            {
                operation.ResponseMessages.Add(new ResponseMessage
                {
                    Code = (int)HttpStatusCode.Unauthorized,
                    Message = "Authentication required"
                });
            }
        }
    }
}