using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger.Annotations;

namespace Swashbuckle.Dummy.Controllers
{
    [SwaggerResponse(400, "Bad request")]
    public class SwaggerAnnotatedController : ApiController
    {
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(int))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Invalid message", typeof(HttpError))]
        public int CreateMessage(Message message)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Message> GetAllMessages()
        {
            throw new NotImplementedException();
        }
    }

    public class Message
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }
}