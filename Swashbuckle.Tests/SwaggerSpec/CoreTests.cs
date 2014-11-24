using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Swashbuckle.Application;
using Swashbuckle.Dummy.Controllers;
using Swashbuckle.Swagger;
using System;

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
                    apis = new object[]
                    {
                        new { path = "/Customers" },
                        new { path = "/Products" }
                    },
                    apiVersion = "1.0"
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
                                    summary = "",
                                    nickname = "Customers_Create",
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
                                    consumes = new []{ "application/json", "text/json", "application/xml", "text/xml", "application/x-www-form-urlencoded" },
                                    type = "integer",
                                    format = "int32"
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
                                    summary = "",
                                    nickname = "Customers_Delete",
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
                                    consumes = new string[]{},
                                    type = "void"
                                },
                                new
                                {
                                    method = "PUT",
                                    summary = "",
                                    nickname = "Customers_Update",
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
                                    consumes = new []{ "application/json", "text/json", "application/xml", "text/xml", "application/x-www-form-urlencoded" },
                                    type = "void"
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
                                    summary = "",
                                    nickname = "Products_FindAll",
                                    parameters = new object[]{},
                                    responseMessages = new object[]{},
                                    produces = new []{ "application/json", "text/json", "application/xml", "text/xml" },
                                    consumes = new object[]{},
                                    type = "array",
                                    items = JObject.Parse("{ $ref: \"Product\" }"),
                                },
                                new
                                {
                                    method = "GET",
                                    summary = "",
                                    nickname = "Products_FindByType",
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
                                    consumes = new object[]{},
                                    type = "array",
                                    items = JObject.Parse("{ $ref: \"Product\" }")
                                },
                                new
                                {
                                    method = "POST",
                                    summary = "",
                                    nickname = "Products_Create",
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
                                    consumes = new []{ "application/json", "text/json", "application/xml", "text/xml", "application/x-www-form-urlencoded" },
                                    type = "integer",
                                    format = "int32"
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
                                    summary = "",
                                    nickname = "Products_GetById",
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
                                    consumes = new object[]{},
                                    type = "Product"
                                }
                           }
                        },
                    }
                });

            Assert.AreEqual(expected.ToString(), declaration.ToString());
        }

        [Test]
        public void It_should_handle_attribute_routes()
        {
            SetUpAttributeRoutes();

            var declaration = Get<JObject>("http://tempuri.org/swagger/api-docs/AttributeRoutes");
            var api = declaration.SelectToken("apis[0]");
            Assert.AreEqual("/subscriptions/{id}/cancel", (string)api["path"]);
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
                    apis = new object[]
                    {
                        new { path = "/deletes" },
                        new { path = "/gets" },
                        new { path = "/posts" },
                        new { path = "/puts" },
                    },
                    apiVersion = "1.0"
                });
            Assert.AreEqual(expected.ToString(), listing.ToString());

            Assert.NotNull(Get<JObject>("http://tempuri.org/swagger/api-docs/deletes"));
            Assert.NotNull(Get<JObject>("http://tempuri.org/swagger/api-docs/gets"));
            Assert.NotNull(Get<JObject>("http://tempuri.org/swagger/api-docs/posts"));
            Assert.NotNull(Get<JObject>("http://tempuri.org/swagger/api-docs/puts"));
        }

        [Test]
        public void It_should_support_customized_api_declaration_ordering()
        {
            _swaggerSpecConfig.SortDeclarationsBy(new DescendingAlphabeticComparer());

            var listing = Get<JObject>("http://tempuri.org/swagger/api-docs/");

            var expected = JObject.FromObject(
                new
                {
                    swaggerVersion = "1.2",
                    apis = new object[]
                    {
                        new { path = "/Products" },
                        new { path = "/Customers" }
                    },
                    apiVersion = "1.0"
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

        [Test]
        public void It_should_support_description_of_additional_api_info()
        {
            _swaggerSpecConfig.ApiInfo(new Info
                {
                    Title = "Title",
                    Description = "Description",
                    TermsOfServiceUrl = "http://tempuri.org/terms",
                    Contact = "contact@tempuri.org",
                    License = "Apache 2.0",
                    LicenseUrl = "http://www.apache.org/licenses/LICENSE-2.0.html"
                });

            var listing = Get<JObject>("http://tempuri.org/swagger/api-docs");
            var info = listing["info"];

            var expected = JObject.FromObject(
                new
                {
                    title = "Title",
                    description = "Description",
                    termsOfServiceUrl = "http://tempuri.org/terms",
                    contact = "contact@tempuri.org",
                    license = "Apache 2.0",
                    licenseUrl = "http://www.apache.org/licenses/LICENSE-2.0.html"
                });

            Assert.AreEqual(expected.ToString(), info.ToString());
        }

        [Test]
        public void It_should_respond_with_a_404_if_declaration_not_found()
        {
            var result = ExecuteGet("http://tempuri.org/swagger/api-docs/NoSuchController");

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public void It_should_handle_actionname_attribute()
        {
            SetUpCustomRouteFor<CustomActionNamesController>("{action}/{id}");

            var declaration = Get<JObject>("http://tempuri.org/swagger/api-docs/CustomActionNames");
            var api = declaration.SelectToken("apis[0]");
            Assert.AreEqual("/TestActionName/{id}", (string)api["path"]);
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