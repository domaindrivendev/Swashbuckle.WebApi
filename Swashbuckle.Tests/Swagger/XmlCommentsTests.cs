using System;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using Swashbuckle.Application;
using Swashbuckle.Dummy.Controllers;
using Swashbuckle.Tests.Swagger;

namespace Swashbuckle.Tests.Swagger
{
    [TestFixture]
    public class XmlCommentsTests : SwaggerTestBase
    {
        public XmlCommentsTests()
            : base("swagger/docs/{apiVersion}")
        {
        }

        [SetUp]
        public void SetUp()
        {
            SetUpDefaultRouteFor<XmlAnnotatedController>();
            SetUpHandler(c => c.IncludeXmlComments(String.Format(@"{0}\XmlComments.xml", AppDomain.CurrentDomain.BaseDirectory)));
        }

        [Test]
        public void It_documents_operations_from_action_summary_and_remarks_tags()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/1.0");

            var postOp = swagger["paths"]["/xmlannotated"]["post"];

            Assert.IsNotNull(postOp["summary"]);
            Assert.AreEqual("Registers a new Account", postOp["summary"].ToString());

            Assert.IsNotNull(postOp["description"]);
            Assert.AreEqual("Create an account to access restricted resources", postOp["description"].ToString());
        }

        [Test]
        public void It_documents_parameters_from_action_param_tags()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/1.0");

            var accountParam = swagger["paths"]["/xmlannotated"]["post"]["parameters"][0];
            Assert.IsNotNull(accountParam["description"]);
            Assert.AreEqual("Details for the account to be created", accountParam["description"].ToString());

            var keywordsParam = swagger["paths"]["/xmlannotated"]["get"]["parameters"][0];
            Assert.IsNotNull(keywordsParam["description"]);
            Assert.AreEqual("List of search keywords", keywordsParam["description"].ToString());
        }

        [Test]
        public void It_documents_schemas_from_type_summary_tags()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/1.0");

            var accountSchema = swagger["definitions"]["Account"];

            Assert.IsNotNull(accountSchema["description"]);
            Assert.AreEqual("Account details", accountSchema["description"].ToString());
        }

        [Test]
        public void It_documents_schema_properties_from_property_summary_tags()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/1.0");

            var usernameProperty = swagger["definitions"]["Account"]["properties"]["Username"];
            Assert.IsNotNull(usernameProperty["description"]);
            Assert.AreEqual("Uniquely identifies the account", usernameProperty["description"].ToString());

            var passwordProperty = swagger["definitions"]["Account"]["properties"]["Password"];
            Assert.IsNotNull(passwordProperty["description"]);
            Assert.AreEqual("For authentication", passwordProperty["description"].ToString());
        }
    }
}