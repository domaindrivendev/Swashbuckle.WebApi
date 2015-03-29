namespace Swashbuckle.Tests.Swagger
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using Swashbuckle.Dummy.Controllers;

    [TestFixture]
    public class ResponseAttributeTests : SwaggerTestBase
    {
        public ResponseAttributeTests()
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
        public void It_documents_responses_from_action_attributes()
        {
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");

            var createResponses = swagger["paths"]["/swaggerannotated"]["post"]["responses"];
            Trace.WriteLine(createResponses);

            var expected = JObject.FromObject(new Dictionary<string, object>()
                {
                    {
                        "200", new
                        {
                            description = "OK",
                            schema = new
                            {
                                format = "int32",
                                type = "integer"
                            }
                        }
                    },
                    {
                        "429", new
                        {
                            description = "Too many requests."
                        }
                    },
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
                        "404", new
                        {
                            description = "Customer not found."
                        }
                    }
                });
            Assert.AreEqual(expected.ToString(), createResponses.ToString());
        }
    }
}
