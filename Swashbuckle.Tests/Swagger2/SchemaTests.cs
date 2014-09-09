using Newtonsoft.Json.Linq;
using System;
using NUnit.Framework;
using Swashbuckle.Dummy.Controllers;
using System.Collections.Generic;
using Swashbuckle.Application;
using Swashbuckle.Configuration;
using Swashbuckle.Dummy.SwaggerExtensions;

namespace Swashbuckle.Tests.Swagger2
{
    [TestFixture]
    public class SchemaTests : HttpMessageHandlerTestFixture<SwaggerDocsHandler>
    {
        private Swagger2Config _swaggerConfig;

        public SchemaTests()
            : base("swagger/docs/{apiVersion}")
        { }

        [SetUp]
        public void SetUp()
        {
            _swaggerConfig = new Swagger2Config();
            _swaggerConfig.ApiVersion("1.0").Title("Test API");

            Configuration.SetSwaggerConfig(_swaggerConfig);
        }

        [Test]
        public void It_should_provide_definition_schemas_for_complex_types()
        {
            AddDefaultRouteFor<ProductsController>();

            var swagger = Get<JObject>("http://tempuri.org/swagger/docs/1.0");

            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new
                {
                    Product = new
                    {
                        required = new string[] { },
                        properties = new
                        {
                            Id = new
                            {
                                format = "int32",
                                type = "integer"
                            },
                            Type = new
                            {
                                @enum = new[] { "Book", "Album" },
                                type = "string"
                            },
                            Description = new
                            {
                                type = "string"
                            },
                            UnitPrice = new
                            {
                                format = "double",
                                type = "number"
                            }
                        },
                        type = "object"
                    }
                });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }
    }
}