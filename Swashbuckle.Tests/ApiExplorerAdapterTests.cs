using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using NUnit.Framework;
using Swashbuckle.Models;
using Swashbuckle.TestApp.App_Start;
using Swashbuckle.TestApp.SwaggerFilters;

namespace Swashbuckle.Tests
{
    public class ApiExplorerAdapterTests
    {
        private List<IOperationSpecFilter> _postFilters;
        private ISwaggerSpec _swaggerSpec;

        [SetUp]
        public void Setup()
        {
            // Get ApiExplorer for TestApp
            var httpConfiguration = new HttpConfiguration();
            WebApiConfig.Register(httpConfiguration);

            _postFilters = new List<IOperationSpecFilter>();
            _swaggerSpec = new ApiExplorerAdapter(
                new ApiExplorer(httpConfiguration),
                new ControllerGroupingStrategy(),
                _postFilters,
                () => "http://tempuri.org");
        }

        [Test]
        public void ItShouldListApiDeclarationsByGroupingStrategy()
        {
            // e.g. By controller name
            var resourceListing = _swaggerSpec.GetResourceListing();
            Assert.AreEqual("1.0", resourceListing.apiVersion);
            Assert.AreEqual("1.2", resourceListing.swaggerVersion);
            Assert.AreEqual(3, resourceListing.apis.Count());

            Assert.IsTrue(resourceListing.apis.Any(a => a.path == "/swagger/api-docs/Orders"), "Orders declaration not listed");
            Assert.IsTrue(resourceListing.apis.Any(a => a.path == "/swagger/api-docs/OrderItems"), "OrderItems declaration not listed");
            Assert.IsTrue(resourceListing.apis.Any(a => a.path == "/swagger/api-docs/Customers"), "Customers declaration not listed");
        }

        [Test]
        public void ItShouldProvideTheListedApiDeclarations()
        {
            ApiDeclaration("/swagger/api-docs/Orders", dec =>
                {
                    Assert.AreEqual("1.2", dec.swaggerVersion);
                    Assert.AreEqual("http://tempuri.org", dec.basePath);
                    Assert.AreEqual("/swagger/api-docs/Orders", dec.resourcePath);
                });

            ApiDeclaration("/swagger/api-docs/OrderItems", dec =>
                {
                    Assert.AreEqual("1.2", dec.swaggerVersion);
                    Assert.AreEqual("http://tempuri.org", dec.basePath);
                    Assert.AreEqual("/swagger/api-docs/OrderItems", dec.resourcePath);
                });

            ApiDeclaration("/swagger/api-docs/Customers", dec =>
                {
                    Assert.AreEqual("1.2", dec.swaggerVersion);
                    Assert.AreEqual("http://tempuri.org", dec.basePath);
                    Assert.AreEqual("/swagger/api-docs/Customers", dec.resourcePath);
                });
        }

        [Test]
        public void ItShouldProvideAnApiSpecForEachUrlPatternInAnApiDeclaration()
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
        public void ItShouldProvideAnOperationSpecForEachMethodOnAUrlPattern()
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
                            Assert.AreEqual("Order", operation.type);
                            Assert.IsNull(operation.@enum);
                            Assert.IsNull(operation.items);
                        });

                    OperationSpec(api, "GET", operation =>
                    {
                        Assert.AreEqual("Orders_GetAll", operation.nickname);
                        Assert.AreEqual("Documentation for 'GetAll'.", operation.summary);
                        Assert.IsNull(operation.notes);
                        Assert.AreEqual("array", operation.type);
                        Assert.IsNull(operation.@enum);
                        Assert.IsNotNull(operation.items);
                        Assert.AreEqual("Order", operation.items["$ref"]);
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
                        Assert.AreEqual("array", operation.type);
                        Assert.IsNull(operation.@enum);
                        Assert.IsNotNull(operation.items);
                        Assert.AreEqual("Order", operation.items["$ref"]);
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
                    Assert.AreEqual("void", operation.type);
                    Assert.IsNull(operation.@enum);
                    Assert.IsNull(operation.items);
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
                    Assert.AreEqual("OrderItem", operation.type);
                    Assert.IsNull(operation.@enum);
                    Assert.IsNull(operation.items);
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
                    Assert.AreEqual("array", operation.type);
                    Assert.IsNull(operation.@enum);
                    Assert.IsNotNull(operation.items);
                    Assert.AreEqual("OrderItem", operation.items["$ref"]);
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
                    Assert.IsNull(operation.type);
                    Assert.IsNull(operation.@enum);
                    Assert.IsNull(operation.items);
                });
            });
        }

        [Test]
        public void ItShouldProvideAParameterSpecForEachParameterInAMethod()
        {
            OperationSpec("/swagger/api-docs/Orders", "/api/orders", 0, "POST", operation =>
                {
                    Assert.AreEqual(1, operation.parameters.Count);

                    ParameterSpec(operation, "order", parameter =>
                        {
                            Assert.AreEqual("body", parameter.paramType);
                            Assert.AreEqual("Documentation for 'order'.", parameter.description);
                            Assert.AreEqual(true, parameter.required);
                            Assert.AreEqual("Order", parameter.type);
                            Assert.IsNull(parameter.@enum);
                            Assert.IsNull(parameter.items);
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
                        Assert.AreEqual("string", parameter.type);
                        Assert.IsNull(parameter.@enum);
                        Assert.IsNull(parameter.items);
                    });

                    ParameterSpec(operation, "bar", parameter =>
                    {
                        Assert.AreEqual("query", parameter.paramType);
                        Assert.AreEqual("Documentation for 'bar'.", parameter.description);
                        Assert.AreEqual(true, parameter.required);
                        Assert.AreEqual("string", parameter.type);
                        Assert.IsNull(parameter.@enum);
                        Assert.IsNull(parameter.items);
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
                        Assert.AreEqual("integer", parameter.type);
                        Assert.IsNull(parameter.@enum);
                        Assert.IsNull(parameter.items);
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
                        Assert.AreEqual("integer", parameter.type);
                        Assert.IsNull(parameter.@enum);
                        Assert.IsNull(parameter.items);
                    });

                    ParameterSpec(operation, "id", parameter =>
                    {
                        Assert.AreEqual("path", parameter.paramType);
                        Assert.AreEqual("Documentation for 'id'.", parameter.description);
                        Assert.AreEqual(true, parameter.required);
                        Assert.AreEqual("integer", parameter.type);
                        Assert.IsNull(parameter.@enum);
                        Assert.IsNull(parameter.items);
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
                        Assert.AreEqual("integer", parameter.type);
                        Assert.IsNull(parameter.@enum);
                        Assert.IsNull(parameter.items);
                    });

                    ParameterSpec(operation, "category", parameter =>
                    {
                        Assert.AreEqual("query", parameter.paramType);
                        Assert.AreEqual("Documentation for 'category'.", parameter.description);
                        Assert.AreEqual(false, parameter.required);
                        Assert.AreEqual("string", parameter.type);
                        Assert.IsNull(parameter.@enum);
                        Assert.IsNull(parameter.items);
                    });
                });

            OperationSpec("/swagger/api-docs/Customers", "/api/customers", 0, "GET", operation =>
                Assert.AreEqual(0, operation.parameters.Count));
        }

        [Test]
        public void ItShouldProvideAModelSpecForComplexTypesInAnApiDeclaration()
        {
            ApiDeclaration("/swagger/api-docs/Orders", dec =>
            {
                Assert.AreEqual(3, dec.models.Count);

                ModelSpec(dec, "Order", model =>
                    Assert.AreEqual("Order", model.id));

//                    AssertModelProperty(model, "Id", property =>
//                        {
//                            Assert.AreEqual("integer", property.type);
//                            Assert.AreEqual("int32", property.format);
//                        }));
            });

//            ApiDeclaration("/swagger/api-docs/OrderItems", dec =>
//            {
//                // 2: /api/orders/{orderId}/items/{id}, /api/orders/{orderId}/items?category={category}
//                Assert.AreEqual(2, dec.apis.Count);
//
//                ApiSpec(dec, "/api/orders/{orderId}/items/{id}", 0, api => Assert.IsNull(api.description));
//                ApiSpec(dec, "/api/orders/{orderId}/items", 0, api => Assert.IsNull(api.description));
//            });
//
//            ApiDeclaration("/swagger/api-docs/Customers", dec =>
//            {
//                // 2: /api/customers
//                Assert.AreEqual(1, dec.apis.Count);
//
//                ApiSpec(dec, "/api/customers", 0, api => Assert.IsNull(api.description));
//            });
        }

//        [Test]
//        public void ItShouldProvideAnOperationSpecForEachOperation()
//        {
//            // Within the orders ApiSpec - GET /api/orders, POST /api/orders
//            Api
//            Assert.AreEqual(2, ordersSpec.operations.Count);
//
//            var getOrdersSpec = ordersSpec.operations.First(op => op.method == "POST");
//            Assert.AreEqual("Documentation for 'Post'.", getOrdersSpec.summary);
//            Assert.AreEqual("Order", getOrdersSpec.type);
//        }

//
//        [Test]
//        public void ItShouldProvideApiSpecForEachUrlPattern()
//        {
            // /api/orders
            //AssertApiSpec("/swagger/api-docs/Orders", "/api/orders", 0, api => ());
//                {
//                    AssertApiOperationSpec(api, "POST", operation =>
//                        {
//                            Assert.AreEqual("Orders", operation.nickname);
//                            Assert.AreEqual("Documentation for 'Post'.", operation.summary);
//                            Assert.AreEqual("Order", operation.type);
//
//                            AssertApiParameterSpec(operation, "order", parameter =>
//                                {
//                                    Assert.AreEqual("body", parameter.paramType);
//                                    Assert.AreEqual("Documentation for 'order'.", parameter.description);
//                                    Assert.AreEqual("Order", parameter.type);
//                                    Assert.AreEqual(true, parameter.required);
//                                    Assert.AreEqual(false, parameter.allowMultiple);
//                                });
//                        });
//
//                    AssertApiOperationSpec(api, "GET", operation =>
//                        {
//                            Assert.AreEqual("Orders", operation.nickname);
//                            Assert.AreEqual("Documentation for 'GetAll'.", operation.summary);
//                            Assert.AreEqual("array[Order]", operation.type);
//                            CollectionAssert.IsEmpty(operation.parameters);
//                        });
//                });
//
//            // /api/orders?foo={foo}&bar={bar}
//            AssertApiSpec("/swagger/api-docs/Orders", "/api/orders", 1, api =>
//                AssertApiOperationSpec(api, "GET", operation =>
//                    {
//                        Assert.AreEqual("Orders", operation.nickname);
//                        Assert.AreEqual("Documentation for 'GetByParams'.", operation.summary);
//                        Assert.AreEqual("array[Order]", operation.type);
//
//                        AssertApiParameterSpec(operation, "foo", parameter =>
//                            {
//                                Assert.AreEqual("query", parameter.paramType);
//                                Assert.AreEqual("Documentation for 'foo'.", parameter.description);
//                                Assert.AreEqual("string", parameter.type);
//                                Assert.AreEqual(true, parameter.required);
//                                Assert.AreEqual(false, parameter.allowMultiple);
//                            });
//
//                        AssertApiParameterSpec(operation, "bar", parameter =>
//                            {
//                                Assert.AreEqual("query", parameter.paramType);
//                                Assert.AreEqual("Documentation for 'bar'.", parameter.description);
//                                Assert.AreEqual("string", parameter.type);
//                                Assert.AreEqual(true, parameter.required);
//                                Assert.AreEqual(false, parameter.allowMultiple);
//                            });
//                    }));
//
//            // /api/orders/{id}
//            AssertApiSpec("/swagger/api-docs/Orders", "/api/orders/{id}", 0, api =>
//                    AssertApiOperationSpec(api, "DELETE", operation =>
//                    {
//                        Assert.AreEqual("Orders", operation.nickname);
//                        Assert.AreEqual("Documentation for 'Delete'.", operation.summary);
//                        Assert.AreEqual("void", operation.type);
//
//                        AssertApiParameterSpec(operation, "id", parameter =>
//                        {
//                            Assert.AreEqual("path", parameter.paramType);
//                            Assert.AreEqual("Documentation for 'id'.", parameter.description);
//                            Assert.AreEqual("integer", parameter.type);
//                            Assert.AreEqual("int32", parameter.format);
//                            Assert.AreEqual(true, parameter.required);
//                            Assert.AreEqual(false, parameter.allowMultiple);
//                        });
//                    }));
//
//            // /api/orders/{orderId}/items/{id}
//            AssertApiSpec("/swagger/api-docs/OrderItems", "/api/orders/{orderId}/items/{id}", 0, api =>
//                AssertApiOperationSpec(api, "GET", operation =>
//                    {
//                        Assert.AreEqual("OrderItems", operation.nickname);
//                        Assert.AreEqual("Documentation for 'GetById'.", operation.summary);
//                        Assert.AreEqual("OrderItem", operation.type);
//
//                        AssertApiParameterSpec(operation, "orderId", parameter =>
//                            {
//                                Assert.AreEqual("path", parameter.paramType);
//                                Assert.AreEqual("Documentation for 'orderId'.", parameter.description);
//                                Assert.AreEqual("integer", parameter.type);
//                                Assert.AreEqual("int32", parameter.format);
//                                Assert.AreEqual(true, parameter.required);
//                                Assert.AreEqual(false, parameter.allowMultiple);
//                            });
//
//                        AssertApiParameterSpec(operation, "id", parameter =>
//                            {
//                                Assert.AreEqual("path", parameter.paramType);
//                                Assert.AreEqual("Documentation for 'id'.", parameter.description);
//                                Assert.AreEqual("integer", parameter.type);
//                                Assert.AreEqual("int32", parameter.format);
//                                Assert.AreEqual(true, parameter.required);
//                                Assert.AreEqual(false, parameter.allowMultiple);
//                            });
//                    }));
//
//            // /api/orders/{orderId}/items
//            AssertApiSpec("/swagger/api-docs/OrderItems", "/api/orders/{orderId}/items", 0, api =>
//                AssertApiOperationSpec(api, "GET", operation =>
//                    {
//                        Assert.AreEqual("OrderItems", operation.nickname);
//                        Assert.AreEqual("Documentation for 'GetAll'.", operation.summary);
//                        Assert.AreEqual("array[OrderItem]", operation.type);
//
//                        AssertApiParameterSpec(operation, "orderId", parameter =>
//                            {
//                                Assert.AreEqual("path", parameter.paramType);
//                                Assert.AreEqual("Documentation for 'orderId'.", parameter.description);
//                                Assert.AreEqual("integer", parameter.type);
//                                Assert.AreEqual("int32", parameter.format);
//                                Assert.AreEqual(true, parameter.required);
//                                Assert.AreEqual(false, parameter.allowMultiple);
//                            });
//
//                        AssertApiParameterSpec(operation, "category", parameter =>
//                            {
//                                Assert.AreEqual("path", parameter.paramType);
//                                Assert.AreEqual("Documentation for 'category'.", parameter.description);
//                                Assert.AreEqual("string", parameter.type);
//                                Assert.AreEqual(false, parameter.allowMultiple);
//                                Assert.AreEqual(new[] { "Category1", "Category2", "Category3" }, parameter.@enum);
//                            });
//                    }));
//
//            // /api/customers
//            AssertApiSpec("/swagger/api-docs/Customers", "/api/customers", 0, api =>
//                AssertApiOperationSpec(api, "GET", operation =>
//                {
//                    Assert.AreEqual("Customers", operation.nickname);
//                    Assert.AreEqual("Documentation for 'GetAll'.", operation.summary);
//                    Assert.AreEqual(null, operation.type);
//                    CollectionAssert.IsEmpty(operation.parameters);
//                }));
//        }
//
//        [Test]
//        public void ItShouldProvideModelSpecForComplexTypes()
//        {
//            AssertModel("/swagger/api-docs/Orders", "Order", model =>
//                AssertModelProperty(model, "Id", property =>
//                    {
//                        Assert.AreEqual("integer", property.type);
//                        Assert.AreEqual("int32", property.format);
//                    }));
//
//            AssertModel("/swagger/api-docs/OrderItems", "OrderItem", model =>
//                {
//                    AssertModelProperty(model, "LineNo", property =>
//                        {
//                            Assert.AreEqual("integer", property.type);
//                            Assert.AreEqual("int32", property.format);
//                        });
//
//                    AssertModelProperty(model, "Product", property =>
//                        Assert.AreEqual("string", property.type));
//
//                    AssertModelProperty(model, "Category", property =>
//                        {
//                            Assert.AreEqual("string", property.type);
//                            Assert.AreEqual(new[] { "Category1", "Category2", "Category3" }, property.@enum);
//                        });
//
//                    AssertModelProperty(model, "Quantity", property =>
//                        {
//                            Assert.AreEqual("integer", property.type);
//                            Assert.AreEqual("int32", property.format);
//                        });
//                });
//        }
//
//        [Test]
//        public void ItShouldExcludeModelSpecForHttpResponseMessage()
//        {
//            AssertApiDeclaration("/swagger/api-docs/Customers", api => CollectionAssert.IsEmpty(api.models));
//        }
//
//        [Test]
//        public void ItShouldApplyConfiguredSpecFilters()
//        {
//            _postFilters.Add(new AddErrorCodeFilter(200, "It's all good!"));
//            _postFilters.Add(new AddErrorCodeFilter(400, "Something's up!"));
//
//            var resourceListing = _swaggerSpec.GetResourceListing();
//            foreach (var path in resourceListing.apis.Select(a => a.path))
//            {
//                foreach (var api in _swaggerSpec.GetApiDeclaration(path).apis)
//                {
//                    foreach (var operation in api.operations)
//                    {
//                        Assert.AreEqual(2, operation.responseMessages.Count);
//                    }
//                }
//            }
//        }

        private void ApiDeclaration(string resourcePath, Action<ApiDeclaration> applyAssertions)
        {
            var declaration = _swaggerSpec.GetApiDeclaration(resourcePath);

            applyAssertions(declaration);
        }

        private void ApiSpec(ApiDeclaration declaration, string apiPath, int index, Action<ApiSpec> applyAssertions)
        {
            var apiSpec = declaration.apis
                .Where(api => api.path == apiPath)
                .ElementAt(index);

            applyAssertions(apiSpec);
        }

        private void ModelSpec(ApiDeclaration declaration, string key, Action<ModelSpec> applyAssertions)
        {
            var modelSpec = declaration.models[key];

            applyAssertions(modelSpec);
        }

        private void ApiSpec(string declarationPath, string apiPath, int index, Action<ApiSpec> applyAssertions)
        {
            var apiSpec = _swaggerSpec.GetApiDeclaration(declarationPath).apis
                .Where(api => api.path == apiPath)
                .ElementAt(index);

            applyAssertions(apiSpec);
        }

        private void OperationSpec(ApiSpec api, string httpMethod, Action<OperationSpec> applyAssertions)
        {
            var operationSpec = api.operations.Single(op => op.method == httpMethod);

            applyAssertions(operationSpec);
        }

        private void OperationSpec(string declarationPath, string apiPath, int index, string httpMethod, Action<OperationSpec> applyAssertions)
        {
            var apiSpec = _swaggerSpec.GetApiDeclaration(declarationPath).apis
                .Where(api => api.path == apiPath)
                .ElementAt(index);

            var operationSpec = apiSpec.operations.Single(op => op.method == httpMethod);

            applyAssertions(operationSpec);
        }

        private void ParameterSpec(OperationSpec operation, string name, Action<ParameterSpec> applyAssertions)
        {
            var parameterSpec = operation.parameters.Single(param => param.name == name);

            applyAssertions(parameterSpec);
        }
    }
}