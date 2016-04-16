using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Swashbuckle.Tests.Owin
{
    [TestFixture]
    public class InMemoryOwinTest : BaseInMemoryOwinSwaggerTest
    {
        [RoutePrefix("v1/callback")]
        [AllowAnonymous]
        public class CallbackController : ApiController
        {
            [Route, HttpGet]
            public HttpResponseMessage Get()
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent("{key:1}", Encoding.UTF8, "application/json");
                return response;
            }
        }

        [Test]
        public async Task It_supports_configuring_single_swagger_endpoint()
        {
            // Given
            UseInMemoryOwinServer(app => new OwinStartup(typeof(CallbackController)).Configuration(app));

            // When
            var swagger = await GetSwaggerDocs();

            // Then
            var postResponses = swagger["paths"]["/v1/callback"]["get"]["operationId"];

            Assert.That(postResponses.Value<string>(), Is.EqualTo("Callback_Get"));
        }

        [Test]
        public async Task It_supports_configuring_multiple_swagger_endpoints()
        {
            // Given
            UseInMemoryOwinServer(app => new MultiSwaggerOwinStartup(typeof(CallbackController)).Configuration(app));

            // When, 
            var swaggerRoot = await GetSwaggerDocs();

            // Then
            var postResponses = swaggerRoot["paths"]["/v1/callback"]["get"]["operationId"];
            Assert.That(postResponses.Value<string>(), Is.EqualTo("Callback_Get"));

            // When, 
            var swaggerCustom = await GetContent("docs/v1/.metadata");

            // Then
            postResponses = swaggerCustom["paths"]["/v1/callback"]["get"]["operationId"];
            Assert.That(postResponses.Value<string>(), Is.EqualTo("Callback_Get"));
        }
    }
}
