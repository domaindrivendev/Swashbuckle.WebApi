using System;
using System.Net;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;

namespace Swashbuckle.Dummy.Controllers
{
    [SwaggerResponse(429, "Too many requests.")]
    public class SwaggerAnnotatedController : ApiController
    {
        [SwaggerResponse(HttpStatusCode.Created, typeof(int))]
        [SwaggerResponse(HttpStatusCode.NotFound, "Customer not found.")]
        public int Create()
        {
            throw new NotImplementedException();
        }
    }
}