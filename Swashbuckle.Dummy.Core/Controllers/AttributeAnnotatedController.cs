namespace Swashbuckle.Dummy.Controllers
{
    using System;
    using System.Net;
    using System.Web.Http;
    using Swashbuckle.Annotations;

    [SwaggerResponse(429, "Too many requests.")]
    public class AttributeAnnotatedController : ApiController
    {
        [SwaggerResponse(HttpStatusCode.Created, typeof(int))]
        [SwaggerResponse(HttpStatusCode.NotFound, "Customer not found.")]
        public int Create(Account account)
        {
            throw new NotImplementedException();
        }
    }
}