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
            _swaggerSpec = new ApiExplorerAdapter(new ApiExplorer(httpConfiguration), () => "http://tempuri.org", _postFilters);
        }

        [Test]
        public void ItShouldListResourcesByControllerName()
        {
            var resourceListing = _swaggerSpec.GetResourceListing();
            Assert.AreEqual("1.0", resourceListing.apiVersion);
            Assert.AreEqual("1.1", resourceListing.swaggerVersion);
            Assert.AreEqual("http://tempuri.org", resourceListing.basePath);

            Assert.AreEqual(2, resourceListing.apis.Count());
            AssertApiDeclarationLink(resourceListing, "/swagger/api-docs/Orders");
            AssertApiDeclarationLink(resourceListing, "/swagger/api-docs/OrderItems");
        }

        [Test]
        public void ItShouldProvideApiDeclarationsForEachController()
        {
            AssertApiDeclaration("/swagger/api-docs/Orders", dec => Assert.AreEqual("http://tempuri.org", dec.basePath));
            AssertApiDeclaration("/swagger/api-docs/OrderItems", dec => Assert.AreEqual("http://tempuri.org", dec.basePath));
        }

        [Test]
        public void ItShouldProvideApiSpecForEachUrlPattern()
        {
            AssertApiSpec("/swagger/api-docs/Orders", "/api/orders", 0, api =>
                {
                    AssertApiOperationSpec(api, "POST", operation =>
                        {
                            Assert.AreEqual("Orders", operation.nickname);
                            Assert.AreEqual("Documentation for 'Post'.", operation.summary);
                            Assert.AreEqual("Order", operation.responseClass);

                            AssertApiParameterSpec(operation, "order", parameter =>
                                {
                                    Assert.AreEqual("body", parameter.paramType);
                                    Assert.AreEqual("Documentation for 'order'.", parameter.description);
                                    Assert.AreEqual("Order", parameter.dataType);
                                    Assert.AreEqual(true, parameter.required);
                                    Assert.AreEqual(false, parameter.allowMultiple);
                                });
                        });

                    AssertApiOperationSpec(api, "GET", operation =>
                        {
                            Assert.AreEqual("Orders", operation.nickname);
                            Assert.AreEqual("Documentation for 'GetAll'.", operation.summary);
                            Assert.AreEqual("List[Order]", operation.responseClass);
                            CollectionAssert.IsEmpty(operation.parameters);
                        });
                });

            AssertApiSpec("/swagger/api-docs/Orders", "/api/orders", 1, api =>
                AssertApiOperationSpec(api, "GET", operation =>
                    {
                        Assert.AreEqual("Orders", operation.nickname);
                        Assert.AreEqual("Documentation for 'GetByParams'.", operation.summary);
                        Assert.AreEqual("List[Order]", operation.responseClass);

                        AssertApiParameterSpec(operation, "foo", parameter =>
                            {
                                Assert.AreEqual("query", parameter.paramType);
                                Assert.AreEqual("Documentation for 'foo'.", parameter.description);
                                Assert.AreEqual("string", parameter.dataType);
                                Assert.AreEqual(true, parameter.required);
                                Assert.AreEqual(false, parameter.allowMultiple);
                            });

                        AssertApiParameterSpec(operation, "bar", parameter =>
                            {
                                Assert.AreEqual("query", parameter.paramType);
                                Assert.AreEqual("Documentation for 'bar'.", parameter.description);
                                Assert.AreEqual("string", parameter.dataType);
                                Assert.AreEqual(true, parameter.required);
                                Assert.AreEqual(false, parameter.allowMultiple);
                            });
                    }));

            AssertApiSpec("/swagger/api-docs/OrderItems", "/api/orders/{orderId}/items/{id}", 0, api =>
                AssertApiOperationSpec(api, "GET", operation =>
                    {
                        Assert.AreEqual("OrderItems", operation.nickname);
                        Assert.AreEqual("Documentation for 'GetById'.", operation.summary);
                        Assert.AreEqual("OrderItem", operation.responseClass);

                        AssertApiParameterSpec(operation, "orderId", parameter =>
                            {
                                Assert.AreEqual("path", parameter.paramType);
                                Assert.AreEqual("Documentation for 'orderId'.", parameter.description);
                                Assert.AreEqual("int", parameter.dataType);
                                Assert.AreEqual(true, parameter.required);
                                Assert.AreEqual(false, parameter.allowMultiple);
                            });

                        AssertApiParameterSpec(operation, "id", parameter =>
                            {
                                Assert.AreEqual("path", parameter.paramType);
                                Assert.AreEqual("Documentation for 'id'.", parameter.description);
                                Assert.AreEqual("int", parameter.dataType);
                                Assert.AreEqual(true, parameter.required);
                                Assert.AreEqual(false, parameter.allowMultiple);
                            });
                    }));

            AssertApiSpec("/swagger/api-docs/OrderItems", "/api/orders/{orderId}/items", 0, api =>
                AssertApiOperationSpec(api, "GET", operation =>
                    {
                        Assert.AreEqual("OrderItems", operation.nickname);
                        Assert.AreEqual("Documentation for 'GetAll'.", operation.summary);
                        Assert.AreEqual("List[OrderItem]", operation.responseClass);

                        AssertApiParameterSpec(operation, "orderId", parameter =>
                            {
                                Assert.AreEqual("path", parameter.paramType);
                                Assert.AreEqual("Documentation for 'orderId'.", parameter.description);
                                Assert.AreEqual("int", parameter.dataType);
                                Assert.AreEqual(true, parameter.required);
                                Assert.AreEqual(false, parameter.allowMultiple);
                            });

                        AssertApiParameterSpec(operation, "category", parameter =>
                            {
                                Assert.AreEqual("path", parameter.paramType);
                                Assert.AreEqual("Documentation for 'category'.", parameter.description);
                                Assert.AreEqual("string", parameter.dataType);
                                Assert.AreEqual(false, parameter.allowMultiple);
                                
                                Assert.IsInstanceOf<EnumeratedValuesSpec>(parameter.allowableValues);
                                var values = (parameter.allowableValues as EnumeratedValuesSpec).values.ToArray();
                                Assert.AreEqual("Category1", values[0]);
                                Assert.AreEqual("Category2", values[1]);
                                Assert.AreEqual("Category3", values[2]);
                            });
                    }));
        }

        [Test]
        public void ItShouldProvideModelSpecForEachComplexType()
        {
            AssertModel("/swagger/api-docs/Orders", "Order", model =>
                AssertModelProperty(model, "Id", property =>
                    {
                        Assert.AreEqual("int", property.type);
                        Assert.AreEqual(true, property.required);
                    }));

            AssertModel("/swagger/api-docs/OrderItems", "OrderItem", model =>
                {
                    AssertModelProperty(model, "LineNo", property =>
                        {
                            Assert.AreEqual("int", property.type);
                            Assert.AreEqual(true, property.required);
                        });

                    AssertModelProperty(model, "Product", property =>
                        {
                            Assert.AreEqual("string", property.type);
                            Assert.AreEqual(true, property.required);
                        });

                    AssertModelProperty(model, "Category", property =>
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

                    AssertModelProperty(model, "Quantity", property =>
                        {
                            Assert.AreEqual("int", property.type);
                            Assert.AreEqual(true, property.required);
                        });
                });
        }

        [Test]
        public void ItShouldApplyConfiguredSpecFilters()
        {
            _postFilters.Add(new AddErrorCodeFilter(200, "It's all good!"));
            _postFilters.Add(new AddErrorCodeFilter(400, "Something's up!"));

            var resourceListing = _swaggerSpec.GetResourceListing();
            foreach (var path in resourceListing.apis.Select(a => a.path))
            {
                foreach (var api in _swaggerSpec.GetApiDeclaration(path).apis)
                {
                    foreach (var operation in api.operations)
                    {
                        Assert.AreEqual(2, operation.errorResponses.Count);
                    }
                }
            }
        }

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

        private void AssertApiSpec(string resourcePath, string apiPath, int apiIndex, Action<ApiSpec> applyAssertions)
        {
            var declaration = _swaggerSpec.GetApiDeclaration(resourcePath);
            var apiSpec = declaration.apis
                .Where(a => a.path == apiPath)
                .ElementAt(apiIndex);

            applyAssertions(apiSpec);
        }

        private void AssertApiOperationSpec(ApiSpec apiSpec, string httpMethod, Action<ApiOperationSpec> applyAssertions)
        {
            var operationSpec = apiSpec.operations.Single(op => op.httpMethod == httpMethod);
            applyAssertions(operationSpec);
        }

        private void AssertApiParameterSpec(ApiOperationSpec operationSpec, string paramName, Action<ApiParameterSpec> applyAssertions)
        {
            var paramSpec = operationSpec.parameters.Single(p => p.name == paramName);
            applyAssertions(paramSpec);
        }

        private void AssertModel(string resourcePath, string modelName, Action<ModelSpec> applyAssertions)
        {
            var declaration = _swaggerSpec.GetApiDeclaration(resourcePath);
            var modelSpec = declaration.models[modelName];
            Assert.IsNotNull(modelSpec);
            applyAssertions(modelSpec);
        }

        private void AssertModelProperty(ModelSpec modelSpec, string propertyName, Action<ModelPropertySpec> applyAssertions)
        {
            var propertySpec = modelSpec.properties[propertyName];
            applyAssertions(propertySpec);
        }
    }
}