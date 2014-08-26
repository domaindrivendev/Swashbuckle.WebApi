using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Swashbuckle.Application;
using Swashbuckle.Dummy.Controllers;
using System.Web.Http;

namespace Swashbuckle.Tests.SwaggerSpec
{
    [TestFixture]
    public class MultipleApiVersionsTests : HttpMessageHandlerTestsBase<SwaggerSpecHandler>
    {
        private SwaggerSpecConfig _swaggerSpecConfig;

        public MultipleApiVersionsTests()
            : base("swagger/{apiVersion}/api-docs")
        { }

        [SetUp]
        public void SetUp()
        {
            _swaggerSpecConfig = new SwaggerSpecConfig();
            Handler = new SwaggerSpecHandler(_swaggerSpecConfig);

            SetUpDefaultRoutesFor(new[] { typeof(ProductsController), typeof(CustomersController) });
        }

        [Test]
        public void It_should_support_multiple_api_versions_by_provided_strategy()
        {
            _swaggerSpecConfig.SupportMultipleApiVersions((apiDesc) =>
                {
                    if (apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName == "Products")
                        return new[] { "1.0", "2.0" };
                    return new[] { "2.0" };
                });

            var v1Listing = Get<JObject>("http://tempuri.org/swagger/1.0/api-docs");
            var expected = JObject.FromObject(
                new
                {
                    swaggerVersion = "1.2",
                    apis = new object[]
                    {
                        new { path = "/Products" }
                    },
                    apiVersion = "1.0"
                });
            Assert.AreEqual(expected.ToString(), v1Listing.ToString());

            var v2Listing = Get<JObject>("http://tempuri.org/swagger/2.0/api-docs");
            expected = JObject.FromObject(
                new
                {
                    swaggerVersion = "1.2",
                    apis = new object[]
                    {
                        new { path = "/Customers" },
                        new { path = "/Products" }
                    },
                    apiVersion = "2.0"
                });
            Assert.AreEqual(expected.ToString(), v2Listing.ToString());
        }

   }
}
