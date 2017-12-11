using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger.Annotations;
using Swashbuckle.Dummy.SwaggerExtensions;

namespace Swashbuckle.Dummy.Controllers
{
    [SwaggerResponse(400, "Bad request")]
    public class SwaggerAnnotatedController : ApiController
    {
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(int))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Invalid message", typeof(HttpError))] 
        public int Create(Message message)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Message> GetAll()
        {
            throw new NotImplementedException();
        }

        [SwaggerOperationFilter(typeof(AddGetMessageExamples))]
        public Message GetById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpPut]
        [SwaggerOperation("UpdateMessage", Tags = new[] { "messages" }, Schemes = new[] { "ws" })]
        
        public void Put([SwaggerDescription("param description")]int id, Message message)
        {
            throw new NotImplementedException();
        }
    }

    [SwaggerSchemaFilter(typeof(AddMessageDefault))]
    public class Message
    {
        [SwaggerDescription("param model description")]
        public string Title { get; set; }
        public string Content { get; set; }
    }
}