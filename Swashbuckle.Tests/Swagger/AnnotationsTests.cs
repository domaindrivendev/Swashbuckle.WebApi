using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Swashbuckle.Dummy.Controllers;

namespace Swashbuckle.Tests.Swagger
{
    [TestFixture]
    public class AnnotationsTests : SwaggerTestBase
    {
        public AnnotationsTests()
            : base("swagger/docs/{apiVersion}")
        {
        }

        [SetUp]
        public void SetUp()
        {
            SetUpDefaultRouteFor<SwaggerAnnotatedController>();

            // Default set-up
            SetUpHandler();
        }

        [Test]
        public void It_documents_responses_from_swagger_response_attributes()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");

            var postResponses = swagger["paths"]["/swaggerannotated"]["post"]["responses"];
            var expected = JObject.FromObject(new Dictionary<string, object>()
                {
                    {
                        "201", new
                        {
                            description = "Created",
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
                            description = "Invalid message",
                            schema = new
                            {
                                type = "object",
                                additionalProperties = JObject.Parse("{ $ref: \"#/definitions/Object\" }") 
                            }
                        }
                    }
                });
            Assert.AreEqual(expected.ToString(), postResponses.ToString());

            var getResponses = swagger["paths"]["/swaggerannotated"]["get"]["responses"];
            expected = JObject.FromObject(new Dictionary<string, object>()
                {
                    {
                        "200", new
                        {
                            description = "OK",
                            schema = new
                            {
                                type = "array",
                                items = JObject.Parse("{ $ref: \"#/definitions/Message\" }") 
                            }
                        }
                    },
                    {
                        "400", new
                        {
                            description = "Bad request"
                        }
                    }
                });
            Assert.AreEqual(expected.ToString(), getResponses.ToString());
        }
    }
}
