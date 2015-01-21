using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Swashbuckle.Application;
using Swashbuckle.Dummy.Controllers;

namespace Swashbuckle.Tests.Swagger
{
    [TestFixture]
    public class FromUriParamsTests : HttpMessageHandlerTestsBase<SwaggerSpecHandler>
    {
        public FromUriParamsTests()
            : base("swagger/api-docs/{resourceName}")
        {}

        [SetUp]
        public void SetUp()
        {
            var swaggerSpecConfig = new SwaggerSpecConfig();
            Handler = new SwaggerSpecHandler(swaggerSpecConfig);

            SetUpDefaultRouteFor<FromUriParamsController>();
        }

        [Test]
        public void It_creates_multiple_query_params_for_from_uri_object_params()
        {
            var declaration = Get<JObject>("http://tempuri.org/swagger/api-docs/FromUriParams");
            var getParams = declaration.SelectToken("apis[0].operations[0].parameters");

            var expectedGetParams = JArray.FromObject(new object[]
            {
                new
                {
                    paramType = "query",
                    name = "currency",
                    required = true,
                    type = "string"
                },
                new
                {
                    paramType = "query",
                    name = "amount",
                    required = true,
                    type = "number",
                    format = "double"
                },
                new
                {
                    paramType = "query",
                    name = "billto.country",
                    required = true,
                    type = "string"
                },
                new
                {
                    paramType = "query",
                    name = "billto.city",
                    required = false,
                    type = "string"
                },
                new
                {
                   paramType = "query",
                   name = "shipto.country",
                   required = false,
                   type = "string"
                },
                new
                {
                    paramType = "query",
                    name = "shipto.city",
                    required = false,
                    type = "string"
                }
            });

            Assert.AreEqual(expectedGetParams.ToString(), getParams.ToString());
        }
    }
}