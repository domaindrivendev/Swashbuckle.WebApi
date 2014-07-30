using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Swashbuckle.Application;
using Swashbuckle.Dummy.Controllers;
using Swashbuckle.Swagger;
using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Swashbuckle.Tests.SwaggerSpec
{
    [TestFixture]
    public class CoreTests : HttpMessageHandlerTestsBase<SwaggerSpecHandler>
    {
        private SwaggerSpecConfig _swaggerSpecConfig;

        public CoreTests()
            : base("swagger/api-docs/{resourceName}")
        {}

        [SetUp]
        public void SetUp()
        {
            _swaggerSpecConfig = new SwaggerSpecConfig();
            Handler = new SwaggerSpecHandler(_swaggerSpecConfig);

            SetUpDefaultRoutesFor(new[] { typeof(ProductsController), typeof(CustomersController) });
        }

        [Test]
        public void It_should_provide_a_listing_with_an_api_per_controller_name()
        {
            var listing = Get<JObject>("http://tempuri.org/swagger/api-docs");

            var expected = JObject.FromObject(
                new
                {
                    swaggerVersion = "1.2",
                    apiVersion = "1.0",
                    apis = new object[]
                    {
                        new { path = "/Customers" },
                        new { path = "/Products" }
                    }
                });

            Assert.AreEqual(expected.ToString(), listing.ToString());
        }

        [Test]
        public void It_should_provide_a_declaration_for_each_listed_api()
        {
            AssertCustomersDeclaration();
            AssertProductsDeclaration();
        }

        private void AssertCustomersDeclaration()
        {
            var declaration = Get<JObject>("http://tempuri.org/swagger/api-docs/Customers");
            declaration.Remove("models"); // models are tested separately

            var expected = JObject.FromObject(
                new
                {
                    swaggerVersion = "1.2",
                    apiVersion = "1.0",
                    basePath = "http://tempuri.org",
                    resourcePath = "/Customers",
                    apis = new object[]
                    {
                        new
                        {
                            path = "/customers",
                            operations = new object[]
                            {
                                new
                                {
                                    method = "POST",
                                    nickname = "Customers_Create",
                                    summary = "",
                                    type = "integer",
                                    format = "int32",
                                    parameters = new object[]
                                    {
                                        new
                                        {
                                            paramType = "body",
                                            name = "customer",
                                            required = true,
                                            type = "Customer",
                                        }
                                    },
                                    responseMessages = new object[]{},
                                    produces = new []{ "application/json", "text/json", "application/xml", "text/xml" },
                                    consumes = new []{ "application/json", "text/json", "application/xml", "text/xml", "application/x-www-form-urlencoded" }
                                }
                            }
                        },
                        new
                        {
                            path = "/customers/{id}",
                            operations = new object[]
                            {
                                new
                                {
                                    method = "DELETE",
                                    nickname = "Customers_Delete",
                                    summary = "",
                                    type = "void",
                                    parameters = new object[]
                                    {
                                        new
                                        {
                                            paramType = "path",
                                            name = "id",
                                            required = true,
                                            type = "integer",
                                            format = "int32"
                                        }
                                    },
                                    responseMessages = new object[]{},
                                    produces = new string[]{},
                                    consumes = new string[]{}
                                },
                                new
                                {
                                    method = "PUT",
                                    nickname = "Customers_Update",
                                    summary = "",
                                    type = "void",
                                    parameters = new object[]
                                    {
                                        new
                                        {
                                            paramType = "path",
                                            name = "id",
                                            required = true,
                                            type = "integer",
                                            format = "int32"
                                        },
                                        new
                                        {
                                            paramType = "body",
                                            name = "customer",
                                            required = true,
                                            type = "Customer",
                                        }
                                    },
                                    responseMessages = new object[]{},
                                    produces = new string[]{},
                                    consumes = new []{ "application/json", "text/json", "application/xml", "text/xml", "application/x-www-form-urlencoded" }
                                }
                            }
                        }
                    }
                });

            Assert.AreEqual(expected.ToString(), declaration.ToString());
        }

        private void AssertProductsDeclaration()
        {
            var declaration = Get<JObject>("http://tempuri.org/swagger/api-docs/Products");
            declaration.Remove("models"); // models are tested separately

            var expected = JObject.FromObject(
                new
                {
                    swaggerVersion = "1.2",
                    apiVersion = "1.0",
                    basePath = "http://tempuri.org",
                    resourcePath = "/Products",
                    apis = new object[]
                    {
                        new
                        {
                            path = "/products",
                            operations = new object[]
                            {
                                new
                                {
                                    method = "GET",
                                    nickname = "Products_FindAll",
                                    summary = "",
                                    type = "array",
                                    items = JObject.Parse("{ $ref: \"Product\" }"),
                                    parameters = new object[]{},
                                    responseMessages = new object[]{},
                                    produces = new []{ "application/json", "text/json", "application/xml", "text/xml" },
                                    consumes = new object[]{}
                                },
                                new
                                {
                                    method = "GET",
                                    nickname = "Products_FindByType",
                                    summary = "",
                                    type = "array",
                                    items = JObject.Parse("{ $ref: \"Product\" }"),
                                    parameters = new object[]
                                    {
                                        new
                                        {
                                            paramType = "query",
                                            name = "type",
                                            required = true,
                                            type = "string",
                                            @enum = new[] { "Book", "Album" }
                                        }
                                    },
                                    responseMessages = new object[]{},
                                    produces = new []{ "application/json", "text/json", "application/xml", "text/xml" },
                                    consumes = new object[]{}
                                },
                                new
                                {
                                    method = "POST",
                                    nickname = "Products_Create",
                                    summary = "",
                                    type = "integer",
                                    format = "int32",
                                    parameters = new object[]
                                    {
                                        new
                                        {
                                            paramType = "body",
                                            name = "product",
                                            required = true,
                                            type = "Product",
                                        }
                                    },
                                    responseMessages = new object[]{},
                                    produces = new []{ "application/json", "text/json", "application/xml", "text/xml" },
                                    consumes = new []{ "application/json", "text/json", "application/xml", "text/xml", "application/x-www-form-urlencoded" }
                                }
                            }
                        },
                        new
                        {
                            path = "/products/{id}",
                            operations = new object[]
                            {
                                new
                                {
                                    method = "GET",
                                    nickname = "Products_GetById",
                                    summary = "",
                                    type = "Product",
                                    parameters = new object[]
                                    {
                                        new
                                        {
                                            paramType = "path",
                                            name = "id",
                                            required = true,
                                            type = "integer",
                                            format = "int32"
                                        }
                                    },
                                    responseMessages = new object[]{},
                                    produces = new []{ "application/json", "text/json", "application/xml", "text/xml" },
                                    consumes = new object[]{}
                                }
                           }
                        },
                    }
                });

            Assert.AreEqual(expected.ToString(), declaration.ToString());
        }

        [Test]
        public void It_should_handle_additional_route_parameters_treating_them_as_required_strings()
        {
            // i.e. route params that are not included in the action signature
            SetUpCustomRouteFor<ProductsController>("{apiVersion}/products");

            var versionParam = Get<JObject>("http://tempuri.org/swagger/api-docs/Products")
                .SelectToken("apis[0].operations[0].parameters[0]");

            var expected = JObject.FromObject(new
                {
                    paramType = "path",
                    name = "apiVersion",
                    required = true,
                    type = "string"
                });

            Assert.AreEqual(expected, versionParam);
        }

        [Test]
        public void It_should_support_customized_base_path_resolution()
        {
            _swaggerSpecConfig.ResolveBasePathUsing((req) => "http://custombasepath.com");

            var declaration = Get<JObject>("http://tempuri.org/swagger/api-docs/Products");
            Assert.AreEqual("http://custombasepath.com", (string)declaration["basePath"]);
        }
        
        [Test]
        public void It_should_support_an_optional_setting_to_ignore_any_actions_marked_obsolete()
        {
            SetUpDefaultRouteFor<ObsoleteActionsController>();

            var declaration = Get<JObject>("http://tempuri.org/swagger/api-docs/ObsoleteActions");
            Assert.IsNotNull(declaration.SelectToken("apis[0].operations[1]"));

            _swaggerSpecConfig.IgnoreObsoleteActions();

            declaration = Get<JObject>("http://tempuri.org/swagger/api-docs/ObsoleteActions");
            Assert.IsNull(declaration.SelectToken("apis[0].operations[1]"));
        }

        [Test]
        public void It_should_support_customized_api_declaration_grouping()
        {
            _swaggerSpecConfig.GroupDeclarationsBy((apiDesc) => String.Format("{0}s", apiDesc.HttpMethod.ToString().ToLower()));

            var listing = Get<JObject>("http://tempuri.org/swagger/api-docs/");

            var expected = JObject.FromObject(
                new
                {
                    swaggerVersion = "1.2",
                    apiVersion = "1.0",
                    apis = new object[]
                    {
                        new { path = "/deletes" },
                        new { path = "/gets" },
                        new { path = "/posts" },
                        new { path = "/puts" },
                    }
                });
            Assert.AreEqual(expected.ToString(), listing.ToString());

            Assert.NotNull(Get<JObject>("http://tempuri.org/swagger/api-docs/deletes"));
            Assert.NotNull(Get<JObject>("http://tempuri.org/swagger/api-docs/gets"));
            Assert.NotNull(Get<JObject>("http://tempuri.org/swagger/api-docs/posts"));
            Assert.NotNull(Get<JObject>("http://tempuri.org/swagger/api-docs/puts"));
        }

        [Test]
        public void It_should_support_customized_api_group_ordering()
        {
            _swaggerSpecConfig.SortDeclarationsBy(new DescendingAlphabeticComparer());

            var listing = Get<JObject>("http://tempuri.org/swagger/api-docs/");

            var expected = JObject.FromObject(
                new
                {
                    swaggerVersion = "1.2",
                    apiVersion = "1.0",
                    apis = new object[]
                    {
                        new { path = "/Products" },
                        new { path = "/Customers" }
                    }
                });
            Assert.AreEqual(expected.ToString(), listing.ToString());

            Assert.NotNull(Get<JObject>("http://tempuri.org/swagger/api-docs/Products"));
            Assert.NotNull(Get<JObject>("http://tempuri.org/swagger/api-docs/Customers"));
        }

        [Test]
        public void It_should_support_configurable_filters_for_modifying_generated_operations()
        {
            _swaggerSpecConfig.OperationFilter<AddResponseCodes>();

            var declaration = Get<JObject>("http://tempuri.org/swagger/api-docs/Customers");

            var expected = JObject.FromObject(new { code = 200, message = "It's all good!" });
            Assert.AreEqual(expected.ToString(),
                declaration.SelectToken("apis[1].operations[0].responseMessages[0]").ToString());
            Assert.AreEqual(expected.ToString(),
                declaration.SelectToken("apis[1].operations[1].responseMessages[0]").ToString());
        }

        class AddResponseCodes : IOperationFilter
        {
            public void Apply(Operation operation, DataTypeRegistry dataTypeRegistry, System.Web.Http.Description.ApiDescription apiDescription)
            {
                operation.ResponseMessages.Add(new ResponseMessage { Code = 200, Message = "It's all good!" });
            }
        }

        class DescendingAlphabeticComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return y.CompareTo(x);
            }
        }
    }
}