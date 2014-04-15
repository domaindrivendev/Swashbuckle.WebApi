using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using NUnit.Framework;
using Swashbuckle.Core;
using Swashbuckle.Core.Application;
using Swashbuckle.Core.Swagger;
using Swashbuckle.TestApp.Core;
using Swashbuckle.TestApp.Core.Models;
using Swashbuckle.TestApp.Core.SwaggerExtensions;

namespace Swashbuckle.Tests
{
    public class ApiExplorerAdapterTests
    {
        private const string RequestedBasePath = "http://tempuri.org";
        private const string RequestedVersion = "1.0";

        private ApiExplorer _apiExplorer;

        [SetUp]
        public void Setup()
        {
            // Get ApiExplorer for TestApp
            var httpConfiguration = new HttpConfiguration();
            WebApiConfig.Register(httpConfiguration);
            _apiExplorer = new ApiExplorer(httpConfiguration);
        }

        [Test]
        public void It_should_generate_a_listing_according_to_provided_strategy()
        {
            var swaggerProvider = GetSwaggerProvider(resourceNameResolver: (apiDesc) => apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName);
            var resourceListing = swaggerProvider.GetListing(RequestedBasePath, RequestedVersion);

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
            var swaggerProvider = GetSwaggerProvider(resourceNameResolver: (apiDesc) => apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName);

            ApiDeclaration(swaggerProvider, "Orders", dec =>
                {
                    Assert.AreEqual("1.2", dec.SwaggerVersion);
                    Assert.AreEqual("http://tempuri.org", dec.BasePath);
                    Assert.AreEqual("/Orders", dec.ResourcePath);
                });

            ApiDeclaration(swaggerProvider, "OrderItems", dec =>
                {
                    Assert.AreEqual("1.2", dec.SwaggerVersion);
                    Assert.AreEqual("http://tempuri.org", dec.BasePath);
                    Assert.AreEqual("/OrderItems", dec.ResourcePath);
                });

            ApiDeclaration(swaggerProvider, "Customers", dec =>
                {
                    Assert.AreEqual("1.2", dec.SwaggerVersion);
                    Assert.AreEqual("http://tempuri.org", dec.BasePath);
                    Assert.AreEqual("/Customers", dec.ResourcePath);
                });

            ApiDeclaration(swaggerProvider, "Products", dec =>
                {
                    Assert.AreEqual("1.2", dec.SwaggerVersion);
                    Assert.AreEqual("http://tempuri.org", dec.BasePath);
                    Assert.AreEqual("/Products", dec.ResourcePath);
                });
        }

        [Test]
        public void It_should_generate_an_api_for_each_url_in_a_declaration()
        {
            var swaggerProvider = GetSwaggerProvider();

            ApiDeclaration(swaggerProvider, "Orders", dec =>
                {
                    // 2: /api/orders, /api/orders/{id}
                    Assert.AreEqual(2, dec.Apis.Count);

                    Api(dec, "/api/orders", api => Assert.IsNull(api.Description));
                    Api(dec, "/api/orders/{id}", api => Assert.IsNull(api.Description));
                });

            ApiDeclaration(swaggerProvider, "OrderItems", dec =>
                {
                    // 2: /api/orders/{orderId}/items/{id}, /api/orders/{orderId}/items?category={category}
                    Assert.AreEqual(2, dec.Apis.Count);

                    Api(dec, "/api/orders/{orderId}/items/{id}", api => Assert.IsNull(api.Description));
                    Api(dec, "/api/orders/{orderId}/items", api => Assert.IsNull(api.Description));
                });

            ApiDeclaration(swaggerProvider, "Customers", dec =>
                {
                    // 2: /api/customers, /api/customers/{id}
                    Assert.AreEqual(2, dec.Apis.Count);

                    Api(dec, "/api/customers", api => Assert.IsNull(api.Description));
                    Api(dec, "/api/customers/{id}", api => Assert.IsNull(api.Description));
                });
        }

        [Test]
        public void It_should_generate_an_operation_for_all_supported_methods_on_a_url()
        {
            var swaggerProvider = GetSwaggerProvider();

            Api(swaggerProvider, "Orders", "/api/orders", api =>
                {
                    // 4: POST /api/orders, GET /api/orders, GET /api/orders?foo={foo}&bar={bar}, DELETE /api/orders
                    Assert.AreEqual(4, api.Operations.Count);

                    Operation(api, "POST", 0, operation =>
                        {
                            Assert.AreEqual("Orders_Post", operation.Nickname);
                            Assert.AreEqual("Documentation for 'Post'.", operation.Summary);
                            Assert.IsNull(operation.Notes);
                            Assert.AreEqual("Order", operation.Type);
                            Assert.IsNull(operation.Format);
                            Assert.IsNull(operation.Items);
                            Assert.IsNull(operation.Enum);
                        });

                    Operation(api, "GET", 0, operation =>
                        {
                            Assert.AreEqual("Orders_GetAll", operation.Nickname);
                            Assert.AreEqual("Documentation for 'GetAll'.", operation.Summary);
                            Assert.IsNull(operation.Notes);
                            Assert.AreEqual("array", operation.Type);
                            Assert.AreEqual("Order", operation.Items.Ref);
                            Assert.IsNull(operation.Format);
                            Assert.IsNull(operation.Enum);
                        });

                    Operation(api, "GET", 1, operation =>
                        {
                            Assert.AreEqual("Orders_GetByParams", operation.Nickname);
                            Assert.AreEqual("Documentation for 'GetByParams'.", operation.Summary);
                            Assert.IsNull(operation.Notes);
                            Assert.AreEqual("array", operation.Type);
                            Assert.AreEqual("Order", operation.Items.Ref);
                            Assert.IsNull(operation.Format);
                            Assert.IsNull(operation.Enum);
                        });

                    Operation(api, "DELETE", 0, operation =>
                        {
                            Assert.AreEqual("Orders_DeleteAll", operation.Nickname);
                            Assert.AreEqual("Documentation for 'DeleteAll'.", operation.Summary);
                            Assert.IsNull(operation.Notes);
                            Assert.AreEqual("void", operation.Type);
                            Assert.IsNull(operation.Items);
                            Assert.IsNull(operation.Format);
                            Assert.IsNull(operation.Enum);
                        });
                });

            Api(swaggerProvider, "Orders", "/api/orders/{id}", api =>
                {
                    // 1: DELETE /api/orders/{id}
                    Assert.AreEqual(1, api.Operations.Count);

                    Operation(api, "DELETE", 0, operation =>
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

            Api(swaggerProvider, "OrderItems", "/api/orders/{orderId}/items/{id}", api =>
                {
                    // 1: GET /api/orders/{orderId}/items/{id}
                    Assert.AreEqual(1, api.Operations.Count);

                    Operation(api, "GET", 0, operation =>
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

            Api(swaggerProvider, "OrderItems", "/api/orders/{orderId}/items", api =>
                {
                    // 2: GET /api/orders/{orderId}/items?category={category}, PUT /api/orders/{orderId}/items
                    Assert.AreEqual(2, api.Operations.Count);

                    Operation(api, "GET", 0, operation =>
                        {
                            Assert.AreEqual("OrderItems_GetAll", operation.Nickname);
                            Assert.AreEqual("Documentation for 'GetAll'.", operation.Summary);
                            Assert.IsNull(operation.Notes);
                            Assert.AreEqual("array", operation.Type);
                            Assert.AreEqual("OrderItem", operation.Items.Ref);
                            Assert.IsNull(operation.Format);
                            Assert.IsNull(operation.Enum);
                        });

                    Operation(api, "PUT", 0, operation =>
                        {
                            Assert.AreEqual("OrderItems_GetByPropertyValues", operation.Nickname);
                            Assert.AreEqual("Documentation for 'GetByPropertyValues'.", operation.Summary);
                            Assert.IsNull(operation.Notes);
                            Assert.AreEqual("array", operation.Type);
                            Assert.AreEqual("OrderItem", operation.Items.Ref);
                            Assert.IsNull(operation.Format);
                            Assert.IsNull(operation.Enum);
                        });
                });

            Api(swaggerProvider, "Customers", "/api/customers", api =>
                {
                    // 1: POST /api/customers
                    Assert.AreEqual(1, api.Operations.Count);

                    Operation(api, "POST", 0, operation =>
                        {
                            Assert.AreEqual("Customers_Post", operation.Nickname);
                            Assert.AreEqual("Documentation for 'Post'.", operation.Summary);
                            Assert.IsNull(operation.Notes);
                            Assert.AreEqual("string", operation.Type);
                            Assert.IsNull(operation.Format);
                            Assert.IsNull(operation.Items);
                            Assert.IsNull(operation.Enum);
                        });
                });

            Api(swaggerProvider, "Customers", "/api/customers/{id}", api =>
                {
                    // 1: GET /api/customers/{id}, DELETE /api/customers/{id}
                    Assert.AreEqual(2, api.Operations.Count);

                    Operation(api, "GET", 0, operation =>
                        {
                            Assert.AreEqual("Customers_Get", operation.Nickname);
                            Assert.AreEqual("Documentation for 'Get'.", operation.Summary);
                            Assert.IsNull(operation.Notes);
                            Assert.AreEqual("Customer", operation.Type);
                            Assert.IsNull(operation.Format);
                            Assert.IsNull(operation.Items);
                            Assert.IsNull(operation.Enum);
                        });

                    Operation(api, "DELETE", 0, operation =>
                        {
                            Assert.AreEqual("Customers_Delete", operation.Nickname);
                            Assert.AreEqual("Documentation for 'Delete'.", operation.Summary);
                            Assert.IsNull(operation.Notes);
                            Assert.AreEqual("string", operation.Type);
                            Assert.IsNull(operation.Format);
                            Assert.IsNull(operation.Items);
                            Assert.IsNull(operation.Enum);
                        });
                });

            Api(swaggerProvider, "Products", "/api/products", api =>
                {
                    // 1: GET /api/products
                    Assert.AreEqual(1, api.Operations.Count);

                    Operation(api, "GET", 0, operation =>
                        {
                            Assert.AreEqual("Products_GetAll", operation.Nickname);
                            Assert.AreEqual("Documentation for 'GetAll'.", operation.Summary);
                            Assert.IsNull(operation.Notes);
                            Assert.AreEqual("array", operation.Type);
                            Assert.AreEqual("Product", operation.Items.Ref);
                            Assert.IsNull(operation.Format);
                            Assert.IsNull(operation.Enum);
                        });
                });
        }

        [Test]
        public void It_should_honor_the_config_setting_to_ignore_obsolete_actions()
        {
            var swaggerProvider = GetSwaggerProvider(ignoreObsoletetActions: true);

            Api(swaggerProvider, "Orders", "/api/orders", api => CollectionAssert.IsEmpty(api.Operations.Where(op => op.Nickname == "Orders_DeleteAll")));
        }

        [Test]
        public void It_should_generate_a_parameter_for_each_parameter_in_a_given_operation()
        {
            var swaggerProvider = GetSwaggerProvider();

            Operation(swaggerProvider, "Orders", "/api/orders", 0, "POST", operation =>
                {
                    Assert.AreEqual(1, operation.Parameters.Count);

                    Parameter(operation, "order", parameter =>
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

            Operation(swaggerProvider, "Orders", "/api/orders", 0, "GET", operation =>
                Assert.AreEqual(0, operation.Parameters.Count));

            Operation(swaggerProvider, "Orders", "/api/orders", 1, "GET", operation =>
                {
                    Assert.AreEqual(2, operation.Parameters.Count);

                    Parameter(operation, "foo", parameter =>
                        {
                            Assert.AreEqual("query", parameter.ParamType);
                            Assert.AreEqual("Documentation for 'foo'.", parameter.Description);
                            Assert.AreEqual(true, parameter.Required);
                            Assert.AreEqual("string", parameter.Type);
                            Assert.IsNull(parameter.Format);
                            Assert.IsNull(parameter.Items);
                            Assert.IsNull(parameter.Enum);
                        });

                    Parameter(operation, "bar", parameter =>
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

            Operation(swaggerProvider, "Orders", "/api/orders/{id}", 0, "DELETE", operation =>
                {
                    Assert.AreEqual(1, operation.Parameters.Count);

                    Parameter(operation, "id", parameter =>
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

            Operation(swaggerProvider, "OrderItems", "/api/orders/{orderId}/items/{id}", 0, "GET", operation =>
                {
                    Assert.AreEqual(2, operation.Parameters.Count);

                    Parameter(operation, "orderId", parameter =>
                        {
                            Assert.AreEqual("path", parameter.ParamType);
                            Assert.AreEqual("Documentation for 'orderId'.", parameter.Description);
                            Assert.AreEqual(true, parameter.Required);
                            Assert.AreEqual("integer", parameter.Type);
                            Assert.AreEqual("int32", parameter.Format);
                            Assert.IsNull(parameter.Items);
                            Assert.IsNull(parameter.Enum);
                        });

                    Parameter(operation, "id", parameter =>
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

            Operation(swaggerProvider, "OrderItems", "/api/orders/{orderId}/items", 0, "GET", operation =>
                {
                    Assert.AreEqual(2, operation.Parameters.Count);

                    Parameter(operation, "orderId", parameter =>
                        {
                            Assert.AreEqual("path", parameter.ParamType);
                            Assert.AreEqual("Documentation for 'orderId'.", parameter.Description);
                            Assert.AreEqual(true, parameter.Required);
                            Assert.AreEqual("integer", parameter.Type);
                            Assert.AreEqual("int32", parameter.Format);
                            Assert.IsNull(parameter.Items);
                            Assert.IsNull(parameter.Enum);
                        });

                    Parameter(operation, "category", parameter =>
                        {
                            Assert.AreEqual("query", parameter.ParamType);
                            Assert.AreEqual("Documentation for 'category'.", parameter.Description);
                            Assert.AreEqual(false, parameter.Required);
                            Assert.AreEqual("string", parameter.Type);
                            Assert.IsNotNull(parameter.Enum);
                            Assert.IsTrue(parameter.Enum.SequenceEqual(new[] {"Category1", "Category2", "Category3"}));
                            Assert.IsNull(parameter.Format);
                            Assert.IsNull(parameter.Items);
                        });
                });

            Operation(swaggerProvider, "OrderItems", "/api/orders/{orderId}/items", 0, "PUT", operation =>
                {
                    Assert.AreEqual(2, operation.Parameters.Count);

                    Parameter(operation, "orderId", parameter =>
                        {
                            Assert.AreEqual("path", parameter.ParamType);
                            Assert.AreEqual("Documentation for 'orderId'.", parameter.Description);
                            Assert.AreEqual(true, parameter.Required);
                            Assert.AreEqual("integer", parameter.Type);
                            Assert.AreEqual("int32", parameter.Format);
                            Assert.IsNull(parameter.Items);
                            Assert.IsNull(parameter.Enum);
                        });

                    Parameter(operation, "propertyValues", parameter =>
                        {
                            Assert.AreEqual("body", parameter.ParamType);
                            Assert.AreEqual("Documentation for 'propertyValues'.", parameter.Description);
                            Assert.AreEqual(true, parameter.Required);
                            Assert.AreEqual("array", parameter.Type);
                            Assert.AreEqual("KeyValuePair[String,String]", parameter.Items.Ref);
                            Assert.IsNull(operation.Format);
                            Assert.IsNull(operation.Enum);
                        });
                });

            Operation(swaggerProvider, "Customers", "/api/customers", 0, "POST", operation =>
                {
                    Assert.AreEqual(1, operation.Parameters.Count);

                    Parameter(operation, "customer", parameter =>
                        {
                            Assert.AreEqual("body", parameter.ParamType);
                            Assert.AreEqual("Documentation for 'customer'.", parameter.Description);
                            Assert.AreEqual(true, parameter.Required);
                            Assert.AreEqual("string", parameter.Type);
                            Assert.IsNull(parameter.Format);
                            Assert.IsNull(parameter.Items);
                            Assert.IsNull(parameter.Enum);
                        });
                });

            Operation(swaggerProvider, "Customers", "/api/customers/{id}", 0, "GET", operation =>
                {
                    Assert.AreEqual(1, operation.Parameters.Count);

                    Parameter(operation, "id", parameter =>
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

            Operation(swaggerProvider, "Customers", "/api/customers/{id}", 0, "DELETE", operation =>
                {
                    Assert.AreEqual(1, operation.Parameters.Count);

                    Parameter(operation, "id", parameter =>
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

            Operation(swaggerProvider, "Products", "/api/products", 0, "GET", operation =>
                CollectionAssert.IsEmpty(operation.Parameters)
                );
        }

        [Test]
        public void It_should_generate_models_for_complex_types_in_a_declaration()
        {
            var swaggerProvider = GetSwaggerProvider();

            ApiDeclaration(swaggerProvider, "Orders", dec =>
                {
                    // 1: Order
                    Assert.AreEqual(4, dec.Models.Count);

                    Model(dec, "Order", model =>
                        {
                            CollectionAssert.AreEqual(new[] {"Id", "Total"}, model.Required);
                            Assert.AreEqual(6, model.Properties.Count);

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

                            ModelProperty(model, "GenericType1", property =>
                                {
                                    Assert.IsNull(property.Type);
                                    Assert.AreEqual("MyGenericType[OrderItem]", property.Ref);
                                    Assert.IsNull(property.Format);
                                    Assert.IsNull(property.Enum);
                                });

                            ModelProperty(model, "GenericType2", property =>
                                {
                                    Assert.IsNull(property.Type);
                                    Assert.AreEqual("MyGenericType[ProductCategory]", property.Ref);
                                    Assert.IsNull(property.Format);
                                    Assert.IsNull(property.Enum);
                                });

                            ModelProperty(model, "TypeWithIndexers", property =>
                                {
                                    Assert.IsNull(property.Type);
                                    Assert.AreEqual("MyTypeWithIndexers", property.Ref);
                                    Assert.IsNull(property.Format);
                                    Assert.IsNull(property.Enum);
                                });
                        });

                    Model(dec, "MyGenericType[OrderItem]", model =>
                        {
                            CollectionAssert.IsEmpty(model.Required);
                            Assert.AreEqual(1, model.Properties.Count);

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
                            Assert.AreEqual(1, model.Properties.Count);

                            ModelProperty(model, "TypeName", property =>
                                {
                                    Assert.AreEqual("string", property.Type);
                                    Assert.IsNull(property.Format);
                                    Assert.IsNull(property.Items);
                                    Assert.IsNull(property.Enum);
                                });
                        });

                    Model(dec, "MyTypeWithIndexers", model =>
                        {
                            CollectionAssert.IsEmpty(model.Required);
                            CollectionAssert.IsEmpty(model.Properties);
                        });
                });

            ApiDeclaration(swaggerProvider, "OrderItems", dec =>
                {
                    // 1: OrderItem
                    Assert.AreEqual(2, dec.Models.Count);

                    Model(dec, "OrderItem", model =>
                        {
                            CollectionAssert.AreEqual(new[] {"LineNo", "Product"}, model.Required);
                            Assert.AreEqual(4, model.Properties.Count);

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
                                    Assert.IsTrue(property.Enum.SequenceEqual(new[] {"Category1", "Category2", "Category3"}));
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

                    Model(dec, "KeyValuePair[String,String]", model =>
                        {
                            CollectionAssert.IsEmpty(model.Required);
                            Assert.AreEqual(2, model.Properties.Count);

                            ModelProperty(model, "Key", property =>
                                {
                                    Assert.AreEqual("string", property.Type);
                                    Assert.IsNull(property.Format);
                                    Assert.IsNull(property.Items);
                                    Assert.IsNull(property.Enum);
                                });

                            ModelProperty(model, "Value", property =>
                                {
                                    Assert.AreEqual("string", property.Type);
                                    Assert.IsNull(property.Format);
                                    Assert.IsNull(property.Items);
                                    Assert.IsNull(property.Enum);
                                });
                        });
                });

            ApiDeclaration(swaggerProvider, "Customers", dec =>
                {
                    Assert.AreEqual(1, dec.Models.Count);

                    Model(dec, "Customer", model =>
                        {
                            CollectionAssert.IsEmpty(model.Required);
                            Assert.AreEqual(2, model.Properties.Count);

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

            ApiDeclaration(swaggerProvider, "Products", dec =>
                {
                    Assert.AreEqual(1, dec.Models.Count);

                    Model(dec, "Product", model =>
                        {
                            CollectionAssert.AreEqual(new[] {"Type"}, model.Required);
                            Assert.AreEqual(3, model.Properties.Count);

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

                            ModelProperty(model, "Type", property =>
                                {
                                    Assert.AreEqual("string", property.Type);
                                    Assert.IsNull(property.Format);
                                    Assert.IsNull(property.Items);
                                    Assert.IsNull(property.Enum);
                                });

                            CollectionAssert.IsEmpty(model.SubTypes);
                        });
                });
        }

        [Test]
        public void It_should_generate_models_for_explicitly_configured_sub_types()
        {
            var productType = new BasePolymorphicType<Product>()
                .DiscriminateBy(p => p.Type)
                .SubType<Book>()
                .SubType<Album>()
                .SubType<Service>(s => s
                    .SubType<Shipping>()
                    .SubType<Packaging>());

            var swaggerProvider = GetSwaggerProvider(polymorphicTypes: new PolymorphicType[] { productType });

            ApiDeclaration(swaggerProvider, "Products", dec =>
                {
                    Assert.AreEqual(6, dec.Models.Count);

                    Model(dec, "Product", model =>
                        {
                            CollectionAssert.AreEqual(new[] {"Type"}, model.Required);
                            Assert.AreEqual(3, model.Properties.Count);

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

                            ModelProperty(model, "Type", property =>
                                {
                                    Assert.AreEqual("string", property.Type);
                                    Assert.IsNull(property.Format);
                                    Assert.IsNull(property.Items);
                                    Assert.IsNull(property.Enum);
                                });

                            CollectionAssert.AreEqual(new[] {"Book", "Album", "Service"}, model.SubTypes);
                            Assert.AreEqual("Type", model.Discriminator);
                        });

                    Model(dec, "Book", model =>
                        {
                            Assert.AreEqual(2, model.Properties.Count);

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
                            Assert.AreEqual(2, model.Properties.Count);

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
                            CollectionAssert.AreEqual(new[] {"Shipping", "Packaging"}, model.SubTypes);
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
        public void It_should_apply_all_configured_operation_filters()
        {
            var operationFilters = new IOperationFilter[] {new AddStandardErrorCodes(), new AddAuthorizationErrorCodes()};
            var swaggerProvider = GetSwaggerProvider(operationFilters: operationFilters);

            Operation(swaggerProvider, "Orders", "/api/orders", 0, "POST", operation =>
                Assert.AreEqual(2, operation.ResponseMessages.Count));

            Operation(swaggerProvider, "Orders", "/api/orders", 0, "GET", operation =>
                Assert.AreEqual(2, operation.ResponseMessages.Count));

            Operation(swaggerProvider, "Orders", "/api/orders", 1, "GET", operation =>
                Assert.AreEqual(2, operation.ResponseMessages.Count));

            Operation(swaggerProvider, "Orders", "/api/orders/{id}", 0, "DELETE", operation =>
                Assert.AreEqual(2, operation.ResponseMessages.Count));

            Operation(swaggerProvider, "OrderItems", "/api/orders/{orderId}/items/{id}", 0, "GET", operation =>
                Assert.AreEqual(2, operation.ResponseMessages.Count));

            Operation(swaggerProvider, "OrderItems", "/api/orders/{orderId}/items", 0, "GET", operation =>
                Assert.AreEqual(2, operation.ResponseMessages.Count));

            Operation(swaggerProvider, "OrderItems", "/api/orders/{orderId}/items", 0, "PUT", operation =>
                Assert.AreEqual(2, operation.ResponseMessages.Count));

            Operation(swaggerProvider, "Customers", "/api/customers/{id}", 0, "GET", operation =>
                Assert.AreEqual(3, operation.ResponseMessages.Count));

            Operation(swaggerProvider, "Customers", "/api/customers/{id}", 0, "DELETE", operation =>
                Assert.AreEqual(3, operation.ResponseMessages.Count));

            Operation(swaggerProvider, "Products", "/api/products", 0, "GET", operation =>
                Assert.AreEqual(2, operation.ResponseMessages.Count));
        }

        private ISwaggerProvider GetSwaggerProvider(
            Func<ApiDescription, string> resourceNameResolver = null,
            IEnumerable<PolymorphicType> polymorphicTypes = null,
            IEnumerable<IOperationFilter> operationFilters = null,
            bool ignoreObsoletetActions = false)
        {
            return new ApiExplorerAdapter(
                _apiExplorer,
                ignoreObsoletetActions,
                (apiDesc, version) => true,
                resourceNameResolver ?? (apiDesc => apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName),
                polymorphicTypes ?? new List<PolymorphicType>(),
                new List<IModelFilter>(),
                operationFilters ?? new List<IOperationFilter>());
        }

        private static void ApiDeclaration(ISwaggerProvider swaggerProvider, string resourceName, Action<ApiDeclaration> applyAssertions)
        {
            var declaration = swaggerProvider.GetDeclaration(RequestedBasePath, RequestedVersion, resourceName);
            applyAssertions(declaration);
        }

        private static void Api(ApiDeclaration declaration, string apiPath, Action<Api> applyAssertions)
        {
            var api = declaration.Apis
                .Single(a => a.Path == apiPath);

            applyAssertions(api);
        }

        private static void Api(ISwaggerProvider swaggerProvider, string resourceName, string apiPath, Action<Api> applyAssertions)
        {
            var api = swaggerProvider.GetDeclaration(RequestedBasePath, RequestedVersion, resourceName).Apis
                .Single(a => a.Path == apiPath);

            applyAssertions(api);
        }

        private static void Operation(ISwaggerProvider swaggerProvider, string resourceName, string apiPath, int index, string httpMethod,
            Action<Operation> applyAssertions)
        {
            var api = swaggerProvider.GetDeclaration(RequestedBasePath, RequestedVersion, resourceName).Apis
                .Single(a => a.Path == apiPath);

            Operation(api, httpMethod, index, applyAssertions);
        }

        private static void Operation(Api api, string httpMethod, int index, Action<Operation> applyAssertions)
        {
            var operation = api.Operations.Where(op => op.Method == httpMethod).ElementAt(index);
            applyAssertions(operation);
        }

        private static void Parameter(Operation operation, string name, Action<Parameter> applyAssertions)
        {
            var parameter = operation.Parameters.Single(param => param.Name == name);
            applyAssertions(parameter);
        }

        private static void Model(ApiDeclaration declaration, string id, Action<DataType> applyAssertions)
        {
            var model = declaration.Models[id];
            applyAssertions(model);
        }

        private static void ModelProperty(DataType model, string name, Action<DataType> applyAssertions)
        {
            var modelProperty = model.Properties[name];
            applyAssertions(modelProperty);
        }
    }
}