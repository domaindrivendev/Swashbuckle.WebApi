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
                    c.PostFilter(new AddErrorCodeFilter(200, "It's all good!"));
                    c.PostFilter(new AddErrorCodeFilter(400, "Something's up!"));
                });

            // Get ApiExplorer for TestApp
            var httpConfiguration = new HttpConfiguration();
            WebApiConfig.Register(httpConfiguration);
            var apiExplorer = new ApiExplorer(httpConfiguration);

            _swaggerSpec = SwaggerGenerator.Instance.Generate(apiExplorer);
        }

        [Test]
        public void It_should_generate_a_listing_according_to_provided_strategy()
        {
            // e.g. Uses ControllerName by default
            var resourceListing = _swaggerSpec.Listing;
            Assert.AreEqual("1.0", resourceListing.apiVersion);
            Assert.AreEqual("1.1", resourceListing.swaggerVersion);
            Assert.AreEqual(3, resourceListing.apis.Count());

            Assert.IsTrue(resourceListing.apis.Any(a => a.path == "/swagger/api-docs/Orders"),
                "Orders declaration not listed");
            Assert.IsTrue(resourceListing.apis.Any(a => a.path == "/swagger/api-docs/OrderItems"),
                "OrderItems declaration not listed");
            Assert.IsTrue(resourceListing.apis.Any(a => a.path == "/swagger/api-docs/Customers"),
                "Customers declaration not listed");
        }

        [Test]
        public void It_should_generate_declarations_according_to_provided_strategy()
        {
            ApiDeclaration("/swagger/api-docs/Orders", dec =>
                {
                    Assert.AreEqual("1.1", dec.swaggerVersion);
                    Assert.AreEqual("http://tempuri.org", dec.basePath);
                    Assert.AreEqual("/swagger/api-docs/Orders", dec.resourcePath);
                });

            ApiDeclaration("/swagger/api-docs/OrderItems", dec =>
                {
                    Assert.AreEqual("1.1", dec.swaggerVersion);
                    Assert.AreEqual("http://tempuri.org", dec.basePath);
                    Assert.AreEqual("/swagger/api-docs/OrderItems", dec.resourcePath);
                });

            ApiDeclaration("/swagger/api-docs/Customers", dec =>
                {
                    Assert.AreEqual("1.1", dec.swaggerVersion);
                    Assert.AreEqual("http://tempuri.org", dec.basePath);
                    Assert.AreEqual("/swagger/api-docs/Customers", dec.resourcePath);
                });
        }

        [Test]
        public void It_should_generate_an_api_spec_for_each_url_in_a_declaration()
        {
            ApiDeclaration("/swagger/api-docs/Orders", dec =>
                {
                    // 3: /api/orders, /api/orders?foo={foo}&bar={bar}, /api/orders/{id}
                    Assert.AreEqual(3, dec.apis.Count);

                    ApiSpec(dec, "/api/orders", 0, api => Assert.IsNull(api.description));
                    ApiSpec(dec, "/api/orders", 1, api => Assert.IsNull(api.description));
                    ApiSpec(dec, "/api/orders/{id}", 0, api => Assert.IsNull(api.description));
                });

            ApiDeclaration("/swagger/api-docs/OrderItems", dec =>
                {
                    // 2: /api/orders/{orderId}/items/{id}, /api/orders/{orderId}/items?category={category}
                    Assert.AreEqual(2, dec.apis.Count);

                    ApiSpec(dec, "/api/orders/{orderId}/items/{id}", 0, api => Assert.IsNull(api.description));
                    ApiSpec(dec, "/api/orders/{orderId}/items", 0, api => Assert.IsNull(api.description));
                });

            ApiDeclaration("/swagger/api-docs/Customers", dec =>
                {
                    // 2: /api/customers
                    Assert.AreEqual(1, dec.apis.Count);

                    ApiSpec(dec, "/api/customers", 0, api => Assert.IsNull(api.description));
                });
        }

        [Test]
        public void It_should_generate_an_operation_spec_for_each_supported_method_on_a_url()
        {
            ApiSpec("/swagger/api-docs/Orders", "/api/orders", 0, api =>
                {
                    // 2: POST /api/orders, GET /api/orders
                    Assert.AreEqual(2, api.operations.Count);

                    OperationSpec(api, "POST", operation =>
                        {
                            Assert.AreEqual("Orders_Post", operation.nickname);
                            Assert.AreEqual("Documentation for 'Post'.", operation.summary);
                            Assert.IsNull(operation.notes);
                            Assert.AreEqual("Order", operation.responseClass);
                        });

                    OperationSpec(api, "GET", operation =>
                        {
                            Assert.AreEqual("Orders_GetAll", operation.nickname);
                            Assert.AreEqual("Documentation for 'GetAll'.", operation.summary);
                            Assert.IsNull(operation.notes);
                            Assert.AreEqual("List[Order]", operation.responseClass);
                        });
                });

            ApiSpec("/swagger/api-docs/Orders", "/api/orders", 1, api =>
                {
                    // 1: GET /api/orders?foo={foo}&bar={bar}
                    Assert.AreEqual(1, api.operations.Count);

                    OperationSpec(api, "GET", operation =>
                        {
                            Assert.AreEqual("Orders_GetByParams", operation.nickname);
                            Assert.AreEqual("Documentation for 'GetByParams'.", operation.summary);
                            Assert.IsNull(operation.notes);
                            Assert.AreEqual("List[Order]", operation.responseClass);
                        });
                });

            ApiSpec("/swagger/api-docs/Orders", "/api/orders/{id}", 0, api =>
                {
                    // 1: DELETE /api/orders/{id}
                    Assert.AreEqual(1, api.operations.Count);

                    OperationSpec(api, "DELETE", operation =>
                        {
                            Assert.AreEqual("Orders_Delete", operation.nickname);
                            Assert.AreEqual("Documentation for 'Delete'.", operation.summary);
                            Assert.IsNull(operation.notes);
                            Assert.AreEqual("void", operation.responseClass);
                        });
                });

            ApiSpec("/swagger/api-docs/OrderItems", "/api/orders/{orderId}/items/{id}", 0, api =>
                {
                    // 1: GET /api/orders/{orderId}/items/{id}
                    Assert.AreEqual(1, api.operations.Count);

                    OperationSpec(api, "GET", operation =>
                        {
                            Assert.AreEqual("OrderItems_GetById", operation.nickname);
                            Assert.AreEqual("Documentation for 'GetById'.", operation.summary);
                            Assert.IsNull(operation.notes);
                            Assert.AreEqual("OrderItem", operation.responseClass);
                        });
                });

            ApiSpec("/swagger/api-docs/OrderItems", "/api/orders/{orderId}/items", 0, api =>
                {
                    // 1: GET /api/orders/{orderId}/items?category={category}
                    Assert.AreEqual(1, api.operations.Count);

                    OperationSpec(api, "GET", operation =>
                        {
                            Assert.AreEqual("OrderItems_GetAll", operation.nickname);
                            Assert.AreEqual("Documentation for 'GetAll'.", operation.summary);
                            Assert.IsNull(operation.notes);
                            Assert.AreEqual("List[OrderItem]", operation.responseClass);
                        });
                });

            ApiSpec("/swagger/api-docs/Customers", "/api/customers", 0, api =>
                {
                    // 1: GET /api/customers
                    Assert.AreEqual(1, api.operations.Count);

                    OperationSpec(api, "GET", operation =>
                        {
                            Assert.AreEqual("Customers_GetAll", operation.nickname);
                            Assert.AreEqual("Documentation for 'GetAll'.", operation.summary);
                            Assert.IsNull(operation.notes);
                            Assert.IsNull(operation.responseClass);
                        });
                });
        }

        [Test]
        public void It_should_generate_a_parameter_spec_for_each_parameter_in_a_given_operation()
        {
            OperationSpec("/swagger/api-docs/Orders", "/api/orders", 0, "POST", operation =>
                {
                    Assert.AreEqual(1, operation.parameters.Count);

                    ParameterSpec(operation, "order", parameter =>
                        {
                            Assert.AreEqual("body", parameter.paramType);
                            Assert.AreEqual("Documentation for 'order'.", parameter.description);
                            Assert.AreEqual(true, parameter.required);
                            Assert.AreEqual("Order", parameter.dataType);
                        });
                });

            OperationSpec("/swagger/api-docs/Orders", "/api/orders", 0, "GET", operation =>
                Assert.AreEqual(0, operation.parameters.Count));

            OperationSpec("/swagger/api-docs/Orders", "/api/orders", 1, "GET", operation =>
                {
                    Assert.AreEqual(2, operation.parameters.Count);

                    ParameterSpec(operation, "foo", parameter =>
                        {
                            Assert.AreEqual("query", parameter.paramType);
                            Assert.AreEqual("Documentation for 'foo'.", parameter.description);
                            Assert.AreEqual(true, parameter.required);
                            Assert.AreEqual("string", parameter.dataType);
                        });

                    ParameterSpec(operation, "bar", parameter =>
                        {
                            Assert.AreEqual("query", parameter.paramType);
                            Assert.AreEqual("Documentation for 'bar'.", parameter.description);
                            Assert.AreEqual(true, parameter.required);
                            Assert.AreEqual("string", parameter.dataType);
                        });
                });

            OperationSpec("/swagger/api-docs/Orders", "/api/orders/{id}", 0, "DELETE", operation =>
                {
                    Assert.AreEqual(1, operation.parameters.Count);

                    ParameterSpec(operation, "id", parameter =>
                        {
                            Assert.AreEqual("path", parameter.paramType);
                            Assert.AreEqual("Documentation for 'id'.", parameter.description);
                            Assert.AreEqual(true, parameter.required);
                            Assert.AreEqual("int", parameter.dataType);
                        });
                });

            OperationSpec("/swagger/api-docs/OrderItems", "/api/orders/{orderId}/items/{id}", 0, "GET", operation =>
                {
                    Assert.AreEqual(2, operation.parameters.Count);

                    ParameterSpec(operation, "orderId", parameter =>
                        {
                            Assert.AreEqual("path", parameter.paramType);
                            Assert.AreEqual("Documentation for 'orderId'.", parameter.description);
                            Assert.AreEqual(true, parameter.required);
                            Assert.AreEqual("int", parameter.dataType);
                        });

                    ParameterSpec(operation, "id", parameter =>
                        {
                            Assert.AreEqual("path", parameter.paramType);
                            Assert.AreEqual("Documentation for 'id'.", parameter.description);
                            Assert.AreEqual(true, parameter.required);
                            Assert.AreEqual("int", parameter.dataType);
                        });
                });

            OperationSpec("/swagger/api-docs/OrderItems", "/api/orders/{orderId}/items", 0, "GET", operation =>
                {
                    Assert.AreEqual(2, operation.parameters.Count);

                    ParameterSpec(operation, "orderId", parameter =>
                        {
                            Assert.AreEqual("path", parameter.paramType);
                            Assert.AreEqual("Documentation for 'orderId'.", parameter.description);
                            Assert.AreEqual(true, parameter.required);
                            Assert.AreEqual("int", parameter.dataType);
                        });

                    ParameterSpec(operation, "category", parameter =>
                        {
                            Assert.AreEqual("query", parameter.paramType);
                            Assert.AreEqual("Documentation for 'category'.", parameter.description);
                            Assert.AreEqual(false, parameter.required);
                            Assert.AreEqual("string", parameter.dataType);
                        });
                });

            OperationSpec("/swagger/api-docs/Customers", "/api/customers", 0, "GET", operation =>
                Assert.AreEqual(0, operation.parameters.Count));
        }

        [Test]
        public void It_should_generate_a_model_spec_for_all_complex_types_in_a_declaration()
        {
            ApiDeclaration("/swagger/api-docs/Orders", dec =>
            {
                // 1: Order
                Assert.AreEqual(1, dec.models.Count);

                Model(dec, "Order", model =>
                    {
                        ModelProperty(model, "Id", property =>
                            {
                                Assert.AreEqual("int", property.type);
                                Assert.AreEqual(true, property.required);
                            });
                        ModelProperty(model, "Description", property =>
                            {
                                Assert.AreEqual("string", property.type);
                                Assert.AreEqual(true, property.required);
                            });
                        ModelProperty(model, "Total", property =>
                            {
                                Assert.AreEqual("double", property.type);
                                Assert.AreEqual(true, property.required);
                            });
                    });
            });

            ApiDeclaration("/swagger/api-docs/OrderItems", dec =>
                {
                    // 1: OrderItem
                    Assert.AreEqual(1, dec.models.Count);

                    Model(dec, "OrderItem", model =>
                        {
                            ModelProperty(model, "LineNo", property =>
                            {
                                Assert.AreEqual("int", property.type);
                                Assert.AreEqual(true, property.required);
                            });
                            
                            ModelProperty(model, "Product", property =>
                            {
                                Assert.AreEqual("string", property.type);
                                Assert.AreEqual(true, property.required);
                            });
                            
                            ModelProperty(model, "Category", property =>
                            {
                                Assert.AreEqual("string", property.type);
                                Assert.AreEqual(true, property.required);
                                Assert.AreEqual("LIST", property.allowableValues.valueType);
                            
                                Assert.IsInstanceOf<EnumeratedValuesSpec>(property.allowableValues);
                                var values = (property.allowableValues as EnumeratedValuesSpec).values.ToArray();
                                Assert.AreEqual("Category1", values[0]);
                                Assert.AreEqual("Category2", values[1]);
                                Assert.AreEqual("Category3", values[2]);
                            });
                            
                            ModelProperty(model, "Quantity", property =>
                            {
                                Assert.AreEqual("int", property.type);
                                Assert.AreEqual(true, property.required);
                            });
                        });
                });

            ApiDeclaration("/swagger/api-docs/Customers", dec => Assert.AreEqual(0, dec.models.Count));
        }

        [Test]
        public void It_should_apply_any_provided_operation_spec_filters()
        {
            // e.g. error code filters (see Setup)
            var resourceListing = _swaggerSpec.Listing;
            foreach (var path in resourceListing.apis.Select(a => a.path))
            {
                foreach (var api in _swaggerSpec.Declarations[path].apis)
                {
                    foreach (var operation in api.operations)
                    {
                        Assert.AreEqual(2, operation.errorResponses.Count);
                    }
                }
            }
        }

        private void ApiDeclaration(string resourcePath, Action<ApiDeclaration> applyAssertions)
        {
            var declaration = _swaggerSpec.Declarations[resourcePath];
            applyAssertions(declaration);
        }

        private void ApiSpec(ApiDeclaration declaration, string apiPath, int index, Action<ApiSpec> applyAssertions)
        {
            var apiSpec = declaration.apis
                .Where(api => api.path == apiPath)
                .ElementAt(index);

            applyAssertions(apiSpec);
        }

        private void ApiSpec(string resourcePath, string apiPath, int index, Action<ApiSpec> applyAssertions)
        {
            var apiSpec = _swaggerSpec.Declarations[resourcePath].apis
                .Where(api => api.path == apiPath)
                .ElementAt(index);

            applyAssertions(apiSpec);
        }

        private void OperationSpec(ApiSpec api, string httpMethod, Action<ApiOperationSpec> applyAssertions)
        {
            var operationSpec = api.operations.Single(op => op.httpMethod == httpMethod);
            applyAssertions(operationSpec);
        }

        private void OperationSpec(string resourcePath, string apiPath, int index, string httpMethod,
            Action<ApiOperationSpec> applyAssertions)
        {
            var apiSpec = _swaggerSpec.Declarations[resourcePath].apis
                .Where(api => api.path == apiPath)
                .ElementAt(index);

            var operationSpec = apiSpec.operations.Single(op => op.httpMethod == httpMethod);
            applyAssertions(operationSpec);
        }

        private void ParameterSpec(ApiOperationSpec operation, string name, Action<ApiParameterSpec> applyAssertions)
        {
            var parameterSpec = operation.parameters.Single(param => param.name == name);
            applyAssertions(parameterSpec);
        }

        private void Model(ApiDeclaration declaration, string id, Action<ModelSpec> applyAssertions)
        {
            var modelSpec = declaration.models[id];
            applyAssertions(modelSpec);
        }

        private void ModelProperty(ModelSpec model, string propertyName, Action<ModelPropertySpec> applyAssertions)
        {
            var propertySpec = model.properties[propertyName];
            applyAssertions(propertySpec);
        }
    }
}