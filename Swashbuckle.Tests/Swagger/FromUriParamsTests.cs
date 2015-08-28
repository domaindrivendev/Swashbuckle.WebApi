using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Swashbuckle.Dummy.Controllers;

namespace Swashbuckle.Tests.Swagger
{
    [TestFixture]
    public class FromUriParamsTests : SwaggerTestBase
    {
        public FromUriParamsTests()
            : base("swagger/docs/{apiVersion}")
        { }

        [SetUp]
        public void SetUp()
        {
            SetUpDefaultRouteFor<FromUriParamsController>();
            SetUpHandler();
        }

        [Test]
        public void It_sets_collection_format_multi_for_array_params()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var headParams = swagger["paths"]["/fromuriparams"]["head"]["parameters"];

            var expectedHeadParams = JArray.FromObject(new object[]
            {
                new
                {
                    name = "currencies",
                    @in = "query",
                    required = true,
                    type = "array",
                    items = new
                    {
                        type = "string"
                    },
                    collectionFormat = "multi"
                }
            });

            Assert.AreEqual(expectedHeadParams.ToString(), headParams.ToString());
        }

        [Test]
        public void It_creates_multiple_query_params_for_from_uri_object_params()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var getParams = swagger["paths"]["/fromuriparams/{id}"]["get"]["parameters"];

            var expectedGetParams = JArray.FromObject(new object[]
            {
                new
                {
                    name = "id",
                    @in = "path",
                    required = true,
                    type = "integer",
                    format = "int32"
                },
                new
                {
                    name = "trx.currency",
                    @in = "query",
                    required = true,
                    type = "string"
                },
                new
                {
                    name = "trx.amount",
                    @in = "query",
                    required = true,
                    type = "number",
                    format = "double"
                },
                new
                {
                    name = "billTo.country",
                    @in = "query",
                    required = true,
                    type = "string"
                },
                new
                {
                    name = "billTo.city",
                    @in = "query",
                    required = false,
                    type = "string"
                },
                new
                {
                   name = "shipTo.country",
                   @in = "query",
                   required = false,
                   type = "string"
                },
                new
                {
                    name = "shipTo.city",
                    @in = "query",
                    required = false,
                    type = "string"
                }
            });

            Assert.AreEqual(expectedGetParams.ToString(), getParams.ToString());
        }

        [Test]
        public void It_sets_collection_format_multi_for_object_with_array_property()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var getParams = swagger["paths"]["/fromuriparams"]["get"]["parameters"];

            var expectedGetParams = JArray.FromObject(new object[]
            {
                new
                {
                    name = "search.transactionIds",
                    @in = "query",
                    required = false,
                    type = "array",
                    items = new
                    {
                        type = "integer",
                        format = "int32"
                    },
                    collectionFormat = "multi"
                }
            });

            Assert.AreEqual(expectedGetParams.ToString(), getParams.ToString());
        }
    }
}