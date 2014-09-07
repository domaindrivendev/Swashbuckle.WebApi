using Newtonsoft.Json.Linq;
using System;
using NUnit.Framework;
using Swashbuckle.Application;
using Swashbuckle.Configuration;
using Swashbuckle.Dummy.Controllers;
using System.Collections.Generic;

namespace Swashbuckle.Tests.Swagger2
{
    [TestFixture]
    public class CoreTests : HttpMessageHandlerTestFixture<SwaggerObjectHandler>
    {
        private Swagger2Config _swaggerConfig;

        public CoreTests()
            : base("swagger/object/{apiVersion}")
        { }

        [SetUp]
        public void SetUp()
        {
            _swaggerConfig = new Swagger2Config();
            _swaggerConfig.ApiVersion("1.0").Title("Test API");

            Configuration.SetSwaggerConfig(_swaggerConfig);
        }

        [Test]
        public void It_should_indicate_swagger_version_2()
        {
            var swagger = Get<JObject>("http://tempuri.org/swagger/object/1.0");

            Assert.AreEqual("2.0", swagger["swagger"].ToString());
        }

        [Test]
        public void It_should_provide_a_host_and_base_path_for_the_api()
        {
            var swagger = Get<JObject>("http://tempuri.org/swagger/object/1.0");
        }

        [Test]
        public void It_should_provide_info_version_and_title()
        {
            var swagger = Get<JObject>("http://tempuri.org/swagger/object/1.0");

            var info = swagger["info"];
            Assert.IsNotNull(info);

            var expected = JObject.FromObject(new
                {
                    version = "1.0",
                    title = "Test API",
                });
            Assert.AreEqual(expected.ToString(), info.ToString());
        }

        [Test]
        public void It_should_provide_additional_info_properties_if_configured()
        {
            _swaggerConfig.ApiVersion("1.0")
                .Title("Test API")
                .Description("A test API")
                .TermsOfService("Test terms")
                .Contact(c => c
                    .Name("Joe Test")
                    .Url("http://tempuri.org/contact")
                    .Email("joe.test@tempuri.org"))
                .License(c => c
                    .Name("Test License")
                    .Url("http://tempuri.org/license"));

            var swagger = Get<JObject>("http://tempuri.org/swagger/object/1.0");

            var info = swagger["info"];
            Assert.IsNotNull(info);

            var expected = JObject.FromObject(new
                {
                    version = "1.0",
                    title = "Test API",
                    description = "A test API",
                    termsOfService = "Test terms",
                    contact = new
                    {
                        name = "Joe Test",
                        url = "http://tempuri.org/contact",
                        email = "joe.test@tempuri.org"
                    },
                    license = new
                    {
                        name = "Test License",
                        url = "http://tempuri.org/license"
                    }
                });
            Assert.AreEqual(expected.ToString(), info.ToString());
        }

        [Test]
        public void It_should_provide_a_description_for_each_path_in_the_api()
        {
            AddDefaultRoutesFor(new[] { typeof(ProductsController), typeof(CustomersController) });

            var swagger = Get<JObject>("http://tempuri.org/swagger/object/1.0");
            var paths = swagger["paths"];

            var expected = JObject.FromObject(new Dictionary<string, object>
                {
                    {
                        "/products", new
                        {
                            get = new
                            {
                                operationId = "Products.GetAllByType",
                                consumes = new object[]{},
                                produces = new []{ "application/json", "text/json", "application/xml", "text/xml" },
                                parameters = new []
                                {
                                    new
                                    {
                                        name = "type",
                                        @in = "query",
                                        required = true,
                                        type = "string"
                                    }
                                }
                            },
                            post = new
                            {
                                operationId = "Products.Create",
                                consumes = new []{ "application/json", "text/json", "application/xml", "text/xml", "application/x-www-form-urlencoded" },
                                produces = new []{ "application/json", "text/json", "application/xml", "text/xml" },
                                parameters = new []
                                {
                                    new
                                    {
                                        name = "product",
                                        @in = "body",
                                        required = true,
                                        schema = JObject.Parse("{ $ref: \"#/definitions/Product\" }")
                                    }
                                }
                            }
                        }
                    },
                    {
                        "/products/{id}", new
                        {
                            get = new
                            {
                                operationId = "Products.GetById",
                                consumes = new object[]{},
                                produces = new []{ "application/json", "text/json", "application/xml", "text/xml" },
                                parameters = new []
                                {
                                    new
                                    {
                                        name = "id",
                                        @in = "path",
                                        required = true,
                                        type = "integer"
                                    }
                                }
                            }
                        }
                    },
                    {
                        "/customers", new
                        {
                            post = new
                            {
                                operationId = "Customers.Create",
                                consumes = new []{ "application/json", "text/json", "application/xml", "text/xml", "application/x-www-form-urlencoded" },
                                produces = new []{ "application/json", "text/json", "application/xml", "text/xml" },
                                parameters = new []
                                {
                                    new
                                    {
                                        name = "customer",
                                        @in = "body",
                                        required = true,
                                        schema = JObject.Parse("{ $ref: \"#/definitions/Customer\" }")
                                    }
                                }
                            }
                        }
                    },
                    {
                        "/customers/{id}", new
                        {
                            put = new
                            {
                                operationId = "Customers.Update",
                                consumes = new []{ "application/json", "text/json", "application/xml", "text/xml", "application/x-www-form-urlencoded" },
                                produces = new string[]{},
                                parameters = new object []
                                {
                                    new
                                    {
                                        name = "id",
                                        @in = "path",
                                        required = true,
                                        type = "integer"
                                    },
                                    new
                                    {
                                        name = "customer",
                                        @in = "body",
                                        required = true,
                                        schema = JObject.Parse("{ $ref: \"#/definitions/Customer\" }")
                                    }
                                }
                            },
                            delete = new
                            {
                                operationId = "Customers.Delete",
                                consumes = new string[]{},
                                produces = new string[]{},
                                parameters = new []
                                {
                                    new
                                    {
                                        name = "id",
                                        @in = "path",
                                        required = true,
                                        type = "integer"
                                    }
                                }
                            }
                        }
                    }
                });

            Assert.AreEqual(expected.ToString(), paths.ToString());
        }

        [Test]
        public void It_should_support_a_setting_to_ignore_obsolete_actions()
        {
            AddDefaultRouteFor<ObsoleteActionsController>();

            _swaggerConfig.IgnoreObsoleteActions();

            var swagger = Get<JObject>("http://tempuri.org/swagger/object/1.0");
            var putOp = swagger["paths"]["/obsoleteactions/{id}"]["put"];
            var deleteOp = swagger["paths"]["/obsoleteactions/{id}"]["delete"];

            Assert.IsNotNull(putOp);
            Assert.IsNull(deleteOp);
        }

        [Test]
        public void It_should_handle_additional_route_parameters()
        {
            // i.e. route params that are not included in the action signature
            AddCustomRouteFor<ProductsController>("{apiVersion}/products");

            var swagger = Get<JObject>("http://tempuri.org/swagger/object/1.0");
            var getParams = swagger["paths"]["/{apiVersion}/products"]["get"]["parameters"];

            var expected = JArray.FromObject(new []
                {
                    new
                    {
                        name = "type",
                        @in = "query",
                        required = true,
                        type = "string"
                    },
                    new
                    {
                        name = "apiVersion",
                        @in = "path",
                        required = true,
                        type = "string"
                    }
                });

            Assert.AreEqual(expected.ToString(), getParams.ToString());
        }

        [Test]
        public void It_should_handle_attribute_routes()
        {
            AddAttributeRoutes();

            var swagger = Get<JObject>("http://tempuri.org/swagger/object/1.0");
            var path = swagger["paths"]["/subscriptions/{id}/cancel"];
            Assert.IsNotNull(path);
        }
    }
}