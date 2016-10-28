using System;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using Swashbuckle.Application;
using Swashbuckle.Dummy.Controllers;
using Swashbuckle.Tests.Swagger;
using Swashbuckle.Dummy.SwaggerExtensions;

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
            SetUpAttributeRoutesFrom(typeof(XmlAnnotatedController).Assembly);
            SetUpDefaultRouteFor<XmlAnnotatedController>();
            SetUpHandler(IncludeXmlComments);
        }

        [Test]
        public void It_documents_operations_from_action_summary_and_remarks_tags_including_paramrefs()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");

            var postOp = swagger["paths"]["/xmlannotated"]["post"];

            Assert.IsNotNull(postOp["summary"]);
            Assert.AreEqual("Registers a new Account based on {account}.", postOp["summary"].ToString());

            Assert.IsNotNull(postOp["description"]);
            Assert.AreEqual("Create an {Swashbuckle.Dummy.Controllers.Account} to access restricted resources", postOp["description"].ToString());
        }

        [Test]
        public void It_documents_parameters_from_action_param_tags()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");

            var accountParam = swagger["paths"]["/xmlannotated"]["post"]["parameters"][0];
            Assert.IsNotNull(accountParam["description"]);
            Assert.AreEqual("Details for the account to be created", accountParam["description"].ToString());

            var keywordsParam = swagger["paths"]["/xmlannotated"]["get"]["parameters"][0];
            Assert.IsNotNull(keywordsParam["description"]);
            Assert.AreEqual("List of search keywords", keywordsParam["description"].ToString());
        }

        [Test]
        public void It_documents_responses_from_action_response_tags()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");

            var createResponses = swagger["paths"]["/xmlannotated"]["post"]["responses"];

            var expected = JObject.FromObject(new Dictionary<string, object>()
                {
                    {
                        "201", new
                        {
                            description = "{account} created",
                            schema = new
                            {
                                format = "int32",
                                type = "integer"
                            }
                        }
                    },
                    {
                        "400", new
                        {
                            description = "Username already in use"
                        }
                    }
                });
            Assert.AreEqual(expected.ToString(), createResponses.ToString());
        }

        [Test]
        public void It_documents_schemas_from_type_summary_tags()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");

            var accountSchema = swagger["definitions"]["Account"];

            Assert.IsNotNull(accountSchema["description"]);
            Assert.AreEqual("Account details", accountSchema["description"].ToString());
        }

        [Test]
        public void It_documents_schema_properties_from_property_summary_tags()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");

            var usernameProperty = swagger["definitions"]["Account"]["properties"]["Username"];
            Assert.IsNotNull(usernameProperty["description"]);
            Assert.AreEqual("Uniquely identifies the account", usernameProperty["description"].ToString());

            var passwordProperty = swagger["definitions"]["Account"]["properties"]["Password"];
            Assert.IsNotNull(passwordProperty["description"]);
            Assert.AreEqual("For authentication", passwordProperty["description"].ToString());
        }

        [Test]
        public void It_documents_schema_properties_including_property_summary_tags_from_base_classes()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");

            var usernameProperty = swagger["definitions"]["SubAccount"]["properties"]["Username"];
            Assert.IsNotNull(usernameProperty["description"]);
            Assert.AreEqual("Uniquely identifies the account", usernameProperty["description"].ToString());
        }

        [Test]
        public void It_documents_schema_properties_favoring_property_summary_tags_from_derived_vs_base_classes()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");

            var usernameProperty = swagger["definitions"]["SubAccount"]["properties"]["AccountID"];
            Assert.IsNotNull(usernameProperty["description"]);
            Assert.AreEqual("The Account ID for SubAccounts should be 7 digits.", usernameProperty["description"].ToString());
        }

        [Test]
        public void It_documents_schema_properties_from_summary_tags_of_complex_type_when_query_parameter_is_annotated_with_fromuri_attribute()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");

            var parameters = swagger["paths"]["/xmlannotated/filter"]["get"]["parameters"];

            var qParam = parameters[0];
            Assert.IsNotNull(qParam["description"]);
            Assert.AreEqual("The search query on which to filter accounts", qParam["description"].ToString());

            var limitParam = parameters[1];
            Assert.IsNotNull(limitParam["description"]);
            Assert.AreEqual("The maximum number of accounts to return", limitParam["description"].ToString());

            var offsetParam = parameters[2];
            Assert.IsNotNull(offsetParam["description"]);
            Assert.AreEqual("Offset into the result", offsetParam["description"].ToString());
        }

        [Test]
        public void It_handles_actions_decorated_with_action_name()
        {
            Configuration.Routes.Clear();
            SetUpCustomRouteFor<XmlAnnotatedController>("XmlAnnotated/{id}/{action}");

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var operation = swagger["paths"]["/XmlAnnotated/{id}/put-on-hold"]["put"];
            Assert.IsNotNull(operation["summary"]);
            Assert.AreEqual("Prevents the account from being used", operation["summary"].ToString());
        }

        [Test]
        public void It_handles_actions_with_array_of_generic_parameters()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var operation = swagger["paths"]["/xmlannotated/{id}/metadata"]["put"];
            Assert.IsNotNull(operation["summary"]);
            Assert.AreEqual("Updates metadata associated with an account", operation["summary"].ToString());
        }

        [Test]
        public void It_handles_nested_class_properties()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var displayNameProperty = swagger["definitions"]["AccountPreferences"]["properties"]["DisplayName"];
            Assert.IsNotNull(displayNameProperty["description"]);
            Assert.AreEqual("Provide a display name to use instead of Username when signed in", displayNameProperty["description"].ToString());
        }

        [Test]
        public void It_handles_json_annotated_properties()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var marketingEmailsProperty = swagger["definitions"]["AccountPreferences"]["properties"]["allow-marketing-emails"];
            Assert.IsNotNull(marketingEmailsProperty["description"]);
            Assert.AreEqual("Flag to indicate if marketing emails may be sent", marketingEmailsProperty["description"].ToString());
        }

        [Test]
        public void It_handles_generic_types()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");

            var genericTypeSchema = swagger["definitions"]["Reward[String]"];
            Assert.NotNull(genericTypeSchema["description"]);
            Assert.AreEqual("A redeemable reward", genericTypeSchema["description"].ToString());

            var genericProperty = genericTypeSchema["properties"]["RewardType"];
            Assert.NotNull(genericProperty["description"]);
            Assert.AreEqual("The reward type", genericProperty["description"].ToString());
        }

        [Test]
        public void It_does_not_error_on_types_with_public_fields()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var valueProperty = swagger["definitions"]["Reward[String]"]["properties"]["value"];
            Assert.IsNull(valueProperty["description"]);
        }

        [Test]
        public void It_does_not_clear_previously_added_responses()
        {
            SetUpHandler(c =>
            {
                IncludeXmlComments(c);
                c.OperationFilter<InternalServerErrorResponseOperationFilter>();
            });

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var responsesProperty = swagger["paths"]["/xmlannotated"]["post"]["responses"];
            Assert.IsNotNull(responsesProperty["500"]);
        }

        private void IncludeXmlComments(SwaggerDocsConfig config)
        {
            config.IncludeXmlComments(String.Format(@"{0}\XmlComments.xml", AppDomain.CurrentDomain.BaseDirectory));
        }
    }
}