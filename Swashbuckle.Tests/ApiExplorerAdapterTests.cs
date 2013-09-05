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
        public void ItShouldListResourcesByControllerName()
        {
            var resourceListing = _swaggerSpec.GetResourceListing();
            Assert.AreEqual("1.0", resourceListing.apiVersion);
            Assert.AreEqual("1.2", resourceListing.swaggerVersion);
            Assert.AreEqual(3, resourceListing.apis.Count());

            AssertApiDeclarationLink(resourceListing, "Orders");
            AssertApiDeclarationLink(resourceListing, "OrderItems");
            AssertApiDeclarationLink(resourceListing, "Customers");
        }

        [Test]
        public void ItShouldProvideApiDeclarationsForEachController()
        {
            AssertApiDeclaration("Orders", dec => Assert.AreEqual("http://tempuri.org", dec.basePath));
            AssertApiDeclaration("OrderItems", dec => Assert.AreEqual("http://tempuri.org", dec.basePath));
            AssertApiDeclaration("Customers", dec => Assert.AreEqual("http://tempuri.org", dec.basePath));
        }
//
//        [Test]
//        public void ItShouldProvideApiSpecForEachUrlPattern()
//        {
//            // /api/orders
//            AssertApiSpec("/swagger/api-docs/Orders", "/api/orders", 0, api =>
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

        private void AssertApiDeclarationLink(ResourceListing resourceListing, string resourcePath)
        {
            var links = resourceListing.apis.Where(a => a.path == resourcePath);
            Assert.AreEqual(1, links.Count());
        }

        private void AssertApiDeclaration(string resourcePath, Action<ApiDeclaration> applyAssertions)
        {
            var declaration = _swaggerSpec.GetApiDeclaration(resourcePath);
            applyAssertions(declaration);
        }
//
//        private void AssertApiSpec(string resourcePath, string apiPath, int apiIndex, Action<ApiSpec> applyAssertions)
//        {
//            var declaration = _swaggerSpec.GetApiDeclaration(resourcePath);
//            var apiSpec = declaration.apis
//                .Where(a => a.path == apiPath)
//                .ElementAt(apiIndex);
//
//            applyAssertions(apiSpec);
//        }
//
//        private void AssertApiOperationSpec(ApiSpec apiSpec, string httpMethod, Action<ApiOperationSpec> applyAssertions)
//        {
//            var operationSpec = apiSpec.operations.Single(op => op.httpMethod == httpMethod);
//            applyAssertions(operationSpec);
//        }
//
//        private void AssertApiParameterSpec(ApiOperationSpec operationSpec, string paramName, Action<ApiParameterSpec> applyAssertions)
//        {
//            var paramSpec = operationSpec.parameters.Single(p => p.name == paramName);
//            applyAssertions(paramSpec);
//        }
//
//        private void AssertModel(string resourcePath, string modelName, Action<ModelSpec> applyAssertions)
//        {
//            var declaration = _swaggerSpec.GetApiDeclaration(resourcePath);
//            var modelSpec = declaration.models[modelName];
//            Assert.IsNotNull(modelSpec);
//            applyAssertions(modelSpec);
//        }
//
//        private void AssertModelProperty(ModelSpec modelSpec, string propertyName, Action<ModelPropertySpec> applyAssertions)
//        {
//            var propertySpec = modelSpec.properties[propertyName];
//            applyAssertions(propertySpec);
//        }
    }
}