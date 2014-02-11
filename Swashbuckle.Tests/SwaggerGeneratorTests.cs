using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using NUnit.Framework;
using Swashbuckle.Models;
using Swashbuckle.TestApp.App_Start;
using Swashbuckle.TestApp.SwaggerExtensions;

namespace Swashbuckle.Tests
{
    public class SwaggerGeneratorTests
    {
        private SwaggerSpec _swaggerSpec;

        [SetUp]
        public void Setup()
        {
            SwaggerSpecConfig.Customize(c =>
                {
                    c.ResolveBasePath(() => "http://tempuri.org");
                    c.PostFilter<AddStandardErrorCodes>();
                    c.PostFilter<AddAuthorizationErrorCodes>();
                });

            // Get ApiExplorer for TestApp
            var httpConfiguration = new HttpConfiguration();
            WebApiConfig.Register(httpConfiguration);
            var apiExplorer = new ApiExplorer(httpConfiguration);

            _swaggerSpec = SwaggerSpec.CreateFrom(apiExplorer);
        }

        [Test]
        public void It_should_generate_a_listing_according_to_provided_strategy()
        {
            // e.g. Uses ControllerName by default
            var resourceListing = _swaggerSpec.Listing;
            Assert.AreEqual("1.0", resourceListing.ApiVersion);
            Assert.AreEqual("1.2", resourceListing.SwaggerVersion);
            Assert.AreEqual(4, resourceListing.Apis.Count());

            Assert.IsTrue(resourceListing.Apis.Any(a => a.Path == "/Orders"),
                "Orders declaration not listed");
            Assert.IsTrue(resourceListing.Apis.Any(a => a.Path == "/OrderItems"),
                "OrderItems declaration not listed");
            Assert.IsTrue(resourceListing.Apis.Any(a => a.Path == "/Customers"),
                "Customers declaration not listed");
            Assert.IsTrue(resourceListing.Apis.Any(a => a.Path == "/Products"),
                "Products declaration not listed");
        }

        [Test]
        public void It_should_generate_declarations_according_to_provided_strategy()
        {
            ApiDeclaration("/Orders", dec =>
                {
                    Assert.AreEqual("1.2", dec.SwaggerVersion);
                    Assert.AreEqual("http://tempuri.org", dec.BasePath);
                    Assert.AreEqual("/Orders", dec.ResourcePath);
                });

            ApiDeclaration("/OrderItems", dec =>
                {
                    Assert.AreEqual("1.2", dec.SwaggerVersion);
                    Assert.AreEqual("http://tempuri.org", dec.BasePath);
                    Assert.AreEqual("/OrderItems", dec.ResourcePath);
                });

            ApiDeclaration("/Customers", dec =>
                {
                    Assert.AreEqual("1.2", dec.SwaggerVersion);
                    Assert.AreEqual("http://tempuri.org", dec.BasePath);
                    Assert.AreEqual("/Customers", dec.ResourcePath);
                });
        }

        [Test]
        public void It_should_generate_an_api_spec_for_each_url_in_a_declaration()
        {
            ApiDeclaration("/Orders", dec =>
                {
                    // 2: /api/orders, /api/orders/{id}
                    Assert.AreEqual(2, dec.Apis.Count);

                    ApiSpec(dec, "/api/orders", api => Assert.IsNull(api.Description));
                    ApiSpec(dec, "/api/orders/{id}", api => Assert.IsNull(api.Description));
                });

            ApiDeclaration("/OrderItems", dec =>
                {
                    // 2: /api/orders/{orderId}/items/{id}, /api/orders/{orderId}/items?category={category}
                    Assert.AreEqual(2, dec.Apis.Count);

                    ApiSpec(dec, "/api/orders/{orderId}/items/{id}", api => Assert.IsNull(api.Description));
                    ApiSpec(dec, "/api/orders/{orderId}/items", api => Assert.IsNull(api.Description));
                });

            ApiDeclaration("/Customers", dec =>
                {
                    // 2: /api/customers, /api/customers/{id}
                    Assert.AreEqual(2, dec.Apis.Count);

                    ApiSpec(dec, "/api/customers", api => Assert.IsNull(api.Description));
                    ApiSpec(dec, "/api/customers/{id}", api => Assert.IsNull(api.Description));
                });
        }

        [Test]
        public void It_should_generate_an_operation_spec_for_each_supported_method_on_a_url()
        {
            ApiSpec("/Orders", "/api/orders", api =>
                {
                    // 3: POST /api/orders, GET /api/orders, GET /api/orders?foo={foo}&bar={bar}
                    Assert.AreEqual(3, api.Operations.Count);

                    OperationSpec(api, 0, "POST", operation =>
                        {
                            Assert.AreEqual("Orders_Post", operation.Nickname);
                            Assert.AreEqual("Documentation for 'Post'.", operation.Summary);
                            Assert.IsNull(operation.Notes);
                            Assert.AreEqual("Order", operation.Type);
                            Assert.IsNull(operation.Format);
                            Assert.IsNull(operation.Items);
                            Assert.IsNull(operation.Enum);
                        });

                    OperationSpec(api, 0, "GET", operation =>
                        {
                            Assert.AreEqual("Orders_GetAll", operation.Nickname);
                            Assert.AreEqual("Documentation for 'GetAll'.", operation.Summary);
                            Assert.IsNull(operation.Notes);
                            Assert.AreEqual("array", operation.Type);
                            Assert.AreEqual("Order", operation.Items.Ref);
                            Assert.IsNull(operation.Format);
                            Assert.IsNull(operation.Enum);
                        });

                    OperationSpec(api, 1, "GET", operation =>
                    {
                        Assert.AreEqual("Orders_GetByParams", operation.Nickname);
                        Assert.AreEqual("Documentation for 'GetByParams'.", operation.Summary);
                        Assert.IsNull(operation.Notes);
                        Assert.AreEqual("array", operation.Type);
                        Assert.AreEqual("Order", operation.Items.Ref);
                        Assert.IsNull(operation.Format);
                        Assert.IsNull(operation.Enum);
                    });
                });

            ApiSpec("/Orders", "/api/orders/{id}", api =>
                {
                    // 1: DELETE /api/orders/{id}
                    Assert.AreEqual(1, api.Operations.Count);

                    OperationSpec(api, 0, "DELETE", operation =>
                        {
                            Assert.AreEqual("Orders_Delete", operation.Nickname);
                            Assert.AreEqual("Documentation for 'Delete'.", operation.Summary);
                            Assert.IsNull(operation.Notes);
                            Assert.AreEqual("void", operation.Type);
                            Assert.IsNull(operation.Items);
                            Assert.IsNull(operation.Format);
                            Assert.IsNull(operation.Enum);
                        });
                });

            ApiSpec("/OrderItems", "/api/orders/{orderId}/items/{id}", api =>
                {
                    // 1: GET /api/orders/{orderId}/items/{id}
                    Assert.AreEqual(1, api.Operations.Count);

                    OperationSpec(api, 0, "GET", operation =>
                        {
                            Assert.AreEqual("OrderItems_GetById", operation.Nickname);
                            Assert.AreEqual("Documentation for 'GetById'.", operation.Summary);
                            Assert.IsNull(operation.Notes);
                            Assert.AreEqual("OrderItem", operation.Type);
                            Assert.IsNull(operation.Format);
                            Assert.IsNull(operation.Items);
                            Assert.IsNull(operation.Enum);
                        });
                });

            ApiSpec("/OrderItems", "/api/orders/{orderId}/items", api =>
                {
                    // 1: GET /api/orders/{orderId}/items?category={category}
                    Assert.AreEqual(1, api.Operations.Count);

                    OperationSpec(api, 0, "GET", operation =>
                        {
                            Assert.AreEqual("OrderItems_GetAll", operation.Nickname);
                            Assert.AreEqual("Documentation for 'GetAll'.", operation.Summary);
                            Assert.IsNull(operation.Notes);
                            Assert.AreEqual("array", operation.Type);
                            Assert.AreEqual("OrderItem", operation.Items.Ref);
                            Assert.IsNull(operation.Format);
                            Assert.IsNull(operation.Enum);
                        });
                });

            ApiSpec("/Customers", "/api/customers", api =>
            {
                // 1: POST /api/customers
                Assert.AreEqual(1, api.Operations.Count);

                OperationSpec(api, 0, "POST", operation =>
                {
                    Assert.AreEqual("Customers_Post", operation.Nickname);
                    Assert.AreEqual("Documentation for 'Post'.", operation.Summary);
                    Assert.IsNull(operation.Notes);
                    Assert.AreEqual("Object", operation.Type);
                    Assert.IsNull(operation.Format);
                    Assert.IsNull(operation.Items);
                    Assert.IsNull(operation.Enum);
                });
            });

            ApiSpec("/Customers", "/api/customers/{id}", api =>
                {
                    // 1: GET /api/customers/{id}, DELETE /api/customers/{id}
                    Assert.AreEqual(2, api.Operations.Count);

                    OperationSpec(api, 0, "GET", operation =>
                        {
                            Assert.AreEqual("Customers_Get", operation.Nickname);
                            Assert.AreEqual("Documentation for 'Get'.", operation.Summary);
                            Assert.IsNull(operation.Notes);
                            Assert.AreEqual("Customer", operation.Type);
                            Assert.IsNull(operation.Format);
                            Assert.IsNull(operation.Items);
                            Assert.IsNull(operation.Enum);
                        });

                    OperationSpec(api, 0, "DELETE", operation =>
                    {
                        Assert.AreEqual("Customers_Delete", operation.Nickname);
                        Assert.AreEqual("Documentation for 'Delete'.", operation.Summary);
                        Assert.IsNull(operation.Notes);
                        Assert.AreEqual("Object", operation.Type);
                        Assert.IsNull(operation.Format);
                        Assert.IsNull(operation.Items);
                        Assert.IsNull(operation.Enum);
                    });
                });
        }

        [Test]
        public void It_should_generate_a_parameter_spec_for_each_parameter_in_a_given_operation()
        {
            OperationSpec("/Orders", "/api/orders", 0, "POST", operation =>
                {
                    Assert.AreEqual(1, operation.Parameters.Count);

                    ParameterSpec(operation, "order", parameter =>
                        {
                            Assert.AreEqual("body", parameter.ParamType);
                            Assert.AreEqual("Documentation for 'order'.", parameter.Description);
                            Assert.AreEqual(true, parameter.Required);
                            Assert.AreEqual("Order", parameter.Type);
                            Assert.IsNull(parameter.Format);
                            Assert.IsNull(parameter.Items);
                            Assert.IsNull(parameter.Enum);
                        });
                });

            OperationSpec("/Orders", "/api/orders", 0, "GET", operation =>
                Assert.AreEqual(0, operation.Parameters.Count));

            OperationSpec("/Orders", "/api/orders", 1, "GET", operation =>
                {
                    Assert.AreEqual(2, operation.Parameters.Count);

                    ParameterSpec(operation, "foo", parameter =>
                        {
                            Assert.AreEqual("query", parameter.ParamType);
                            Assert.AreEqual("Documentation for 'foo'.", parameter.Description);
                            Assert.AreEqual(true, parameter.Required);
                            Assert.AreEqual("string", parameter.Type);
                            Assert.IsNull(parameter.Format);
                            Assert.IsNull(parameter.Items);
                            Assert.IsNull(parameter.Enum);
                        });

                    ParameterSpec(operation, "bar", parameter =>
                        {
                            Assert.AreEqual("query", parameter.ParamType);
                            Assert.AreEqual("Documentation for 'bar'.", parameter.Description);
                            Assert.AreEqual(true, parameter.Required);
                            Assert.AreEqual("string", parameter.Type);
                            Assert.IsNull(parameter.Format);
                            Assert.IsNull(parameter.Items);
                            Assert.IsNull(parameter.Enum);
                        });
                });

            OperationSpec("/Orders", "/api/orders/{id}", 0, "DELETE", operation =>
                {
                    Assert.AreEqual(1, operation.Parameters.Count);

                    ParameterSpec(operation, "id", parameter =>
                        {
                            Assert.AreEqual("path", parameter.ParamType);
                            Assert.AreEqual("Documentation for 'id'.", parameter.Description);
                            Assert.AreEqual(true, parameter.Required);
                            Assert.AreEqual("integer", parameter.Type);
                            Assert.AreEqual("int32", parameter.Format);
                            Assert.IsNull(parameter.Items);
                            Assert.IsNull(parameter.Enum);
                        });
                });

            OperationSpec("/OrderItems", "/api/orders/{orderId}/items/{id}", 0, "GET", operation =>
                {
                    Assert.AreEqual(2, operation.Parameters.Count);

                    ParameterSpec(operation, "orderId", parameter =>
                        {
                            Assert.AreEqual("path", parameter.ParamType);
                            Assert.AreEqual("Documentation for 'orderId'.", parameter.Description);
                            Assert.AreEqual(true, parameter.Required);
                            Assert.AreEqual("integer", parameter.Type);
                            Assert.AreEqual("int32", parameter.Format);
                            Assert.IsNull(parameter.Items);
                            Assert.IsNull(parameter.Enum);
                        });

                    ParameterSpec(operation, "id", parameter =>
                        {
                            Assert.AreEqual("path", parameter.ParamType);
                            Assert.AreEqual("Documentation for 'id'.", parameter.Description);
                            Assert.AreEqual(true, parameter.Required);
                            Assert.AreEqual("integer", parameter.Type);
                            Assert.AreEqual("int32", parameter.Format);
                            Assert.IsNull(parameter.Items);
                            Assert.IsNull(parameter.Enum);
                        });
                });

            OperationSpec("/OrderItems", "/api/orders/{orderId}/items", 0, "GET", operation =>
                {
                    Assert.AreEqual(2, operation.Parameters.Count);

                    ParameterSpec(operation, "orderId", parameter =>
                        {
                            Assert.AreEqual("path", parameter.ParamType);
                            Assert.AreEqual("Documentation for 'orderId'.", parameter.Description);
                            Assert.AreEqual(true, parameter.Required);
                            Assert.AreEqual("integer", parameter.Type);
                            Assert.AreEqual("int32", parameter.Format);
                            Assert.IsNull(parameter.Items);
                            Assert.IsNull(parameter.Enum);
                        });

                    ParameterSpec(operation, "category", parameter =>
                        {
                            Assert.AreEqual("query", parameter.ParamType);
                            Assert.AreEqual("Documentation for 'category'.", parameter.Description);
                            Assert.AreEqual(false, parameter.Required);
                            Assert.AreEqual("string", parameter.Type);
                            Assert.IsNotNull(parameter.Enum);
                            Assert.IsTrue(parameter.Enum.SequenceEqual(new[] { "Category1", "Category2", "Category3" }));
                            Assert.IsNull(parameter.Format);
                            Assert.IsNull(parameter.Items);
                        });
                });

            OperationSpec("/Customers", "/api/customers", 0, "POST", operation =>
                {
                    Assert.AreEqual(1, operation.Parameters.Count);

                    ParameterSpec(operation, "customer", parameter =>
                        {
                            Assert.AreEqual("body", parameter.ParamType);
                            Assert.AreEqual("Documentation for 'customer'.", parameter.Description);
                            Assert.AreEqual(true, parameter.Required);
                            Assert.AreEqual("Object", parameter.Type);
                            Assert.IsNull(parameter.Format);
                            Assert.IsNull(parameter.Items);
                            Assert.IsNull(parameter.Enum);
                        });
                });

            OperationSpec("/Customers", "/api/customers/{id}", 0, "GET", operation =>
                {
                    Assert.AreEqual(1, operation.Parameters.Count);

                    ParameterSpec(operation, "id", parameter =>
                        {
                            Assert.AreEqual("path", parameter.ParamType);
                            Assert.AreEqual("Documentation for 'id'.", parameter.Description);
                            Assert.AreEqual(true, parameter.Required);
                            Assert.AreEqual("integer", parameter.Type);
                            Assert.AreEqual("int32", parameter.Format);
                            Assert.IsNull(parameter.Items);
                            Assert.IsNull(parameter.Enum);
                        });
                });

            OperationSpec("/Customers", "/api/customers/{id}", 0, "DELETE", operation =>
            {
                Assert.AreEqual(1, operation.Parameters.Count);

                ParameterSpec(operation, "id", parameter =>
                {
                    Assert.AreEqual("path", parameter.ParamType);
                    Assert.AreEqual("Documentation for 'id'.", parameter.Description);
                    Assert.AreEqual(true, parameter.Required);
                    Assert.AreEqual("integer", parameter.Type);
                    Assert.AreEqual("int32", parameter.Format);
                    Assert.IsNull(parameter.Items);
                    Assert.IsNull(parameter.Enum);
                });
            });
        }

        [Test]
        public void It_should_generate_model_specs_for_all_complex_types_in_a_declaration_including_sub_types()
        {
            ApiDeclaration("/Orders", dec =>
            {
                // 1: Order
                Assert.AreEqual(3, dec.Models.Count);

                Model(dec, "Order", model =>
                    {
                        CollectionAssert.AreEqual(new[] { "Id", "Total" }, model.Required);

                        ModelProperty(model, "Id", property =>
                            {
                                Assert.AreEqual("integer", property.Type);
                                Assert.AreEqual("int32", property.Format);
                                Assert.IsNull(property.Items);
                                Assert.IsNull(property.Enum);
                            });
                        ModelProperty(model, "Description", property =>
                            {
                                Assert.AreEqual("string", property.Type);
                                Assert.IsNull(property.Format);
                                Assert.IsNull(property.Items);
                                Assert.IsNull(property.Enum);
                            });
                        ModelProperty(model, "Total", property =>
                            {
                                Assert.AreEqual("number", property.Type);
                                Assert.AreEqual("double", property.Format);
                                Assert.IsNull(property.Items);
                                Assert.IsNull(property.Enum);
                            });
                    });

                Model(dec, "MyGenericType[OrderItem]", model =>
                    {
                        CollectionAssert.IsEmpty(model.Required);

                        ModelProperty(model, "TypeName", property =>
                            {
                                Assert.AreEqual("string", property.Type);
                                Assert.IsNull(property.Format);
                                Assert.IsNull(property.Items);
                                Assert.IsNull(property.Enum);
                            });
                    });

                Model(dec, "MyGenericType[ProductCategory]", model =>
                    {
                        CollectionAssert.IsEmpty(model.Required);

                        ModelProperty(model, "TypeName", property =>
                            {
                                Assert.AreEqual("string", property.Type);
                                Assert.IsNull(property.Format);
                                Assert.IsNull(property.Items);
                                Assert.IsNull(property.Enum);
                            });
                    });
            });

            ApiDeclaration("/OrderItems", dec =>
                {
                    // 1: OrderItem
                    Assert.AreEqual(1, dec.Models.Count);

                    Model(dec, "OrderItem", model =>
                        {
                            CollectionAssert.AreEqual(new[] { "LineNo", "Product" }, model.Required);

                            ModelProperty(model, "LineNo", property =>
                            {
                                Assert.AreEqual("integer", property.Type);
                                Assert.AreEqual("int32", property.Format);
                                Assert.IsNull(property.Items);
                                Assert.IsNull(property.Enum);
                            });

                            ModelProperty(model, "Product", property =>
                            {
                                Assert.AreEqual("string", property.Type);
                                Assert.IsNull(property.Format);
                                Assert.IsNull(property.Items);
                                Assert.IsNull(property.Enum);
                            });

                            ModelProperty(model, "Category", property =>
                            {
                                Assert.AreEqual("string", property.Type);
                                Assert.IsNotNull(property.Enum);
                                Assert.IsTrue(property.Enum.SequenceEqual(new[] { "Category1", "Category2", "Category3" }));
                                Assert.IsNull(property.Format);
                                Assert.IsNull(property.Items);
                            });

                            ModelProperty(model, "Quantity", property =>
                            {
                                Assert.AreEqual("integer", property.Type);
                                Assert.AreEqual("int32", property.Format);
                                Assert.IsNull(property.Items);
                                Assert.IsNull(property.Enum);
                            });
                        });
                });

            ApiDeclaration("/Customers", dec =>
                {
                    Assert.AreEqual(2, dec.Models.Count);

                    Model(dec, "Object", model => CollectionAssert.IsEmpty(model.Required));

                    Model(dec, "Customer", model =>
                        {
                            CollectionAssert.IsEmpty(model.Required);

                            ModelProperty(model, "Id", property =>
                                {
                                    Assert.AreEqual("integer", property.Type);
                                    Assert.AreEqual("int32", property.Format);
                                    Assert.IsNull(property.Items);
                                    Assert.IsNull(property.Enum);
                                });

                            ModelProperty(model, "Associates", property =>
                                {
                                    Assert.AreEqual("array", property.Type);
                                    Assert.AreEqual("Customer", property.Items.Ref);
                                    Assert.IsNull(property.Format);
                                    Assert.IsNull(property.Enum);
                                });
                        });

                });

            ApiDeclaration("/Products", dec =>
            {
                Assert.AreEqual(6, dec.Models.Count);

                Model(dec, "Product", model =>
                {
                    ModelProperty(model, "Id", property =>
                    {
                        Assert.AreEqual("integer", property.Type);
                        Assert.AreEqual("int32", property.Format);
                        Assert.IsNull(property.Items);
                        Assert.IsNull(property.Enum);
                    });

                    ModelProperty(model, "Price", property =>
                    {
                        Assert.AreEqual("number", property.Type);
                        Assert.AreEqual("double", property.Format);
                        Assert.IsNull(property.Items);
                        Assert.IsNull(property.Enum);
                    });

                    CollectionAssert.AreEqual(new[] { "Book", "Album", "Service" }, model.SubTypes);
                });

                Model(dec, "Book", model =>
                {
                    ModelProperty(model, "Title", property =>
                    {
                        Assert.AreEqual("string", property.Type);
                        Assert.IsNull(property.Format);
                        Assert.IsNull(property.Items);
                        Assert.IsNull(property.Enum);
                    });

                    ModelProperty(model, "Author", property =>
                    {
                        Assert.AreEqual("string", property.Type);
                        Assert.IsNull(property.Format);
                        Assert.IsNull(property.Items);
                        Assert.IsNull(property.Enum);
                    });

                    CollectionAssert.IsEmpty(model.SubTypes);
                });

                Model(dec, "Album", model =>
                {
                    ModelProperty(model, "Name", property =>
                    {
                        Assert.AreEqual("string", property.Type);
                        Assert.IsNull(property.Format);
                        Assert.IsNull(property.Items);
                        Assert.IsNull(property.Enum);
                    });

                    ModelProperty(model, "Artist", property =>
                    {
                        Assert.AreEqual("string", property.Type);
                        Assert.IsNull(property.Format);
                        Assert.IsNull(property.Items);
                        Assert.IsNull(property.Enum);
                    });

                    CollectionAssert.IsEmpty(model.SubTypes);
                });

                Model(dec, "Service", model =>
                {
                    CollectionAssert.IsEmpty(model.Properties);
                    CollectionAssert.AreEqual(new[] { "Shipping", "Packaging" }, model.SubTypes);
                });

                Model(dec, "Shipping", model =>
                {
                    CollectionAssert.IsEmpty(model.Properties);
                    CollectionAssert.IsEmpty(model.SubTypes);
                });

                Model(dec, "Packaging", model =>
                {
                    CollectionAssert.IsEmpty(model.Properties);
                    CollectionAssert.IsEmpty(model.SubTypes);
                });
            });
        }

        [Test]
        public void It_should_apply_any_provided_operation_spec_filters()
        {
            OperationSpec("/Orders", "/api/orders", 0, "POST", operation =>
                Assert.AreEqual(2, operation.ResponseMessages.Count));

            OperationSpec("/Orders", "/api/orders", 0, "GET", operation =>
                Assert.AreEqual(2, operation.ResponseMessages.Count));

            OperationSpec("/Orders", "/api/orders", 1, "GET", operation =>
                Assert.AreEqual(2, operation.ResponseMessages.Count));

            OperationSpec("/Orders", "/api/orders/{id}", 0, "DELETE", operation =>
                Assert.AreEqual(2, operation.ResponseMessages.Count));

            OperationSpec("/OrderItems", "/api/orders/{orderId}/items/{id}", 0, "GET", operation =>
                Assert.AreEqual(2, operation.ResponseMessages.Count));

            OperationSpec("/OrderItems", "/api/orders/{orderId}/items", 0, "GET", operation =>
                Assert.AreEqual(2, operation.ResponseMessages.Count));

            OperationSpec("/Customers", "/api/customers/{id}", 0, "GET", operation =>
                Assert.AreEqual(3, operation.ResponseMessages.Count));

            OperationSpec("/Customers", "/api/customers/{id}", 0, "DELETE", operation =>
                Assert.AreEqual(3, operation.ResponseMessages.Count));
        }

        private void ApiDeclaration(string resourcePath, Action<ApiDeclaration> applyAssertions)
        {
            var declaration = _swaggerSpec.Declarations[resourcePath];
            applyAssertions(declaration);
        }

        private void ApiSpec(ApiDeclaration declaration, string apiPath, Action<ApiSpec> applyAssertions)
        {
            var apiSpec = declaration.Apis
                .Single(api => api.Path == apiPath);

            applyAssertions(apiSpec);
        }

        private void ApiSpec(string resourcePath, string apiPath, Action<ApiSpec> applyAssertions)
        {
            var apiSpec = _swaggerSpec.Declarations[resourcePath].Apis
                .Single(api => api.Path == apiPath);

            applyAssertions(apiSpec);
        }

        private void OperationSpec(ApiSpec api, int index, string httpMethod, Action<OperationSpec> applyAssertions)
        {
            var operationSpec = api.Operations.Where(op => op.Method == httpMethod).ElementAt(index);
            applyAssertions(operationSpec);
        }

        private void OperationSpec(string resourcePath, string apiPath, int index, string httpMethod,
            Action<OperationSpec> applyAssertions)
        {
            var apiSpec = _swaggerSpec.Declarations[resourcePath].Apis
                .Single(api => api.Path == apiPath);

            OperationSpec(apiSpec, index, httpMethod, applyAssertions);
        }

        private void ParameterSpec(OperationSpec operation, string name, Action<ParameterSpec> applyAssertions)
        {
            var parameterSpec = operation.Parameters.Single(param => param.Name == name);
            applyAssertions(parameterSpec);
        }

        private void Model(ApiDeclaration declaration, string id, Action<ModelSpec> applyAssertions)
        {
            var modelSpec = declaration.Models[id];
            applyAssertions(modelSpec);
        }

        private void ModelProperty(ModelSpec model, string name, Action<ModelSpec> applyAssertions)
        {
            var modelPropertySpec = model.Properties[name];
            applyAssertions(modelPropertySpec);
        }
    }
}