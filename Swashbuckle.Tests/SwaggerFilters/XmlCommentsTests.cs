using System;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Swashbuckle.Application;
using Swashbuckle.Dummy.Controllers;

namespace Swashbuckle.Tests.SwaggerFilters
{
    [TestFixture]
    public class XmlCommentsTests : HttpMessageHandlerTestFixture<SwaggerDocsHandler>
    {
        private SwaggerDocsConfig _swaggerDocsConfig;

        public XmlCommentsTests()
            : base("swagger/docs/{apiVersion}")
        {
        }

        [SetUp]
        public void SetUp()
        {
            _swaggerDocsConfig = new SwaggerDocsConfig();
            _swaggerDocsConfig.SingleApiVersion("1.0", "Test API");
            _swaggerDocsConfig.IncludeXmlComments(String.Format(@"{0}\XmlComments.xml", AppDomain.CurrentDomain.BaseDirectory));
            

            Configuration.SetSwaggerDocsConfig(_swaggerDocsConfig);
            AddDefaultRouteFor<XmlAnnotatedController>();
        }

        [Test]
        public void It_should_document_operations_using_xml_action_summaries_and_remarks()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/1.0");

            var postOp = swagger["paths"]["/xmlannotated"]["post"];

            Assert.IsNotNull(postOp["summary"]);
            Assert.AreEqual("Registers a new Account", postOp["summary"].ToString());

            Assert.IsNotNull(postOp["description"]);
            Assert.AreEqual("With a registered account, you can access restricted resources", postOp["description"].ToString());
        }
    }
}