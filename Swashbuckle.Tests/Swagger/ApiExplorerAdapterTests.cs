using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using NUnit.Framework;
using Swashbuckle.Application;
using Swashbuckle.Dummy.Models;
using Swashbuckle.Dummy.SwaggerExtensions;
using Swashbuckle.Swagger;

namespace Swashbuckle.Tests.Swagger
{
    public class ApiExplorerAdapterTests
    {
        private const string RequestedBasePath = "http://tempuri.org";
        private const string RequestedVersion = "1.0";

        private ApiExplorer _apiExplorer;

        [TestFixtureSetUp]
        public void Setup()
        {
            // Get dummy ApiExplorer
            var config = new HttpConfiguration();
            Dummy.WebApiConfig.Register(config);
            _apiExplorer = new ApiExplorer(config);
            config.EnsureInitialized();
        }

        [Test]
        public void It_should_generate_a_listing_grouped_by_the_configured_strategy()
        {
            var swaggerProvider = GetSwaggerProvider(resourceNameResolver: (apiDesc) => apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName);
            var resourceListing = swaggerProvider.GetListing(RequestedBasePath, RequestedVersion);

            Assert.AreEqual("1.0", resourceListing.ApiVersion);
            Assert.AreEqual("1.2", resourceListing.SwaggerVersion);
            Assert.AreEqual(3, resourceListing.Apis.Count());

            Assert.IsTrue(resourceListing.Apis.Any(a => a.Path == "/Products"),
                "Products declaration not listed");
            Assert.IsTrue(resourceListing.Apis.Any(a => a.Path == "/Customers"),
                "Customers declaration not listed");
            Assert.IsTrue(resourceListing.Apis.Any(a => a.Path == "/RandomStuff"),
                "RandomStuff declaration not listed");
        }

        [Test]
        public void It_should_generate_declarations_grouped_by_the_configured_strategy()
        {
            var swaggerProvider = GetSwaggerProvider(resourceNameResolver: (apiDesc) => apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName);

            ApiDeclaration(swaggerProvider, "Products", dec =>
                {
                    Assert.AreEqual("1.2", dec.SwaggerVersion);
                    Assert.AreEqual("http://tempuri.org", dec.BasePath);
                    Assert.AreEqual("/Products", dec.ResourcePath);
                });

            ApiDeclaration(swaggerProvider, "Customers", dec =>
                {
                    Assert.AreEqual("1.2", dec.SwaggerVersion);
                    Assert.AreEqual("http://tempuri.org", dec.BasePath);
                    Assert.AreEqual("/Customers", dec.ResourcePath);
                });

            ApiDeclaration(swaggerProvider, "RandomStuff", dec =>
            {
                Assert.AreEqual("1.2", dec.SwaggerVersion);
                Assert.AreEqual("http://tempuri.org", dec.BasePath);
                Assert.AreEqual("/RandomStuff", dec.ResourcePath);
            });
        }

        [Test]
        public void It_should_generate_an_api_for_each_uri_in_a_declaration()
        {
            var swaggerProvider = GetSwaggerProvider();

            ApiDeclaration(swaggerProvider, "Products", dec =>
                {
                    Assert.AreEqual(2, dec.Apis.Count);

                    Api(dec, "/products", api => Assert.IsNull(api.Description));

                    Api(dec, "/products/{id}/suspend", api => Assert.IsNull(api.Description));
                });

            ApiDeclaration(swaggerProvider, "Customers", dec =>
                {
                    Assert.AreEqual(2, dec.Apis.Count);

                    Api(dec, "/customers", api => Assert.IsNull(api.Description));

                    Api(dec, "/customers/{id}", api => Assert.IsNull(api.Description));
                });

            ApiDeclaration(swaggerProvider, "RandomStuff", dec =>
            {
                Assert.AreEqual(3, dec.Apis.Count);

                Api(dec, "/kittens", api => Assert.IsNull(api.Description));

                Api(dec, "/unicorns", api => Assert.IsNull(api.Description));

                Api(dec, "/unicorns/{id}", api => Assert.IsNull(api.Description));
            });
        }
       
        [Test]
        public void It_should_generate_an_operation_for_each_verb_on_a_given_uri()
        {
            var swaggerProvider = GetSwaggerProvider();

            Api(swaggerProvider, "Products", "/products", api =>
                {
                    // 2: GET /products, GET /products?type={type}
                    Assert.AreEqual(2, api.Operations.Count);

                    Operation(api, "GET", 0, operation =>
                        {
                            Assert.AreEqual("Products_GetAll", operation.Nickname);
                            Assert.IsNull(operation.Summary);
                            Assert.IsNull(operation.Notes);
                            Assert.AreEqual("array", operation.Type);
                            Assert.IsNull(operation.Format);
                            Assert.AreEqual("Product", operation.Items.Ref);
                            Assert.IsNull(operation.Enum);

                            CollectionAssert.IsEmpty(operation.Parameters);
                        });

                    Operation(api, "GET", 1, operation =>
                    {
                        Assert.AreEqual("Products_GetByType", operation.Nickname);
                        Assert.IsNull(operation.Summary);
                        Assert.IsNull(operation.Notes);
                        Assert.AreEqual("array", operation.Type);
                        Assert.IsNull(operation.Format);
                        Assert.AreEqual("Product", operation.Items.Ref);
                        Assert.IsNull(operation.Enum);

                        Assert.AreEqual(2, operation.Parameters.Count);

                        Parameter(operation, "type", parameter =>
                            {
                                Assert.AreEqual("query", parameter.ParamType);
                                Assert.IsNull(parameter.Description);
                                Assert.AreEqual(true, parameter.Required);
                                Assert.AreEqual("string", parameter.Type);
                                Assert.IsNull(parameter.Format);
                                Assert.IsNull(parameter.Items);
                                CollectionAssert.AreEqual(new[] { "Book", "Album", "Shipping", "Packaging" }, parameter.Enum);
                            });

                        Parameter(operation, "maxPrice", parameter =>
                        {
                            Assert.AreEqual("query", parameter.ParamType);
                            Assert.IsNull(parameter.Description);
                            Assert.AreEqual(false, parameter.Required);
                            Assert.AreEqual("number", parameter.Type);
                            Assert.AreEqual("double", parameter.Format);
                            Assert.IsNull(parameter.Items);
                            Assert.IsNull(parameter.Enum);
                        });
                    });
                });

            Api(swaggerProvider, "Products", "/products/{id}/suspend", api =>
                {
                    // 1: PUT /products/{id}/suspend
                    Assert.AreEqual(1, api.Operations.Count);

                    Operation(api, "PUT", 0, operation =>
                        {
                            Assert.AreEqual("Products_Suspend", operation.Nickname);
                            Assert.IsNull(operation.Summary);
                            Assert.IsNull(operation.Notes);
                            Assert.AreEqual("void", operation.Type);
                            Assert.IsNull(operation.Format);
                            Assert.IsNull(operation.Items);
                            Assert.IsNull(operation.Enum);

                            Assert.AreEqual(1, operation.Parameters.Count);

                            Parameter(operation, "id", parameter =>
                                {
                                    Assert.AreEqual("path", parameter.ParamType);
                                    Assert.IsNull(parameter.Description);
                                    Assert.AreEqual(true, parameter.Required);
                                    Assert.AreEqual("integer", parameter.Type);
                                    Assert.AreEqual("int32", parameter.Format);
                                    Assert.IsNull(parameter.Items);
                                    Assert.IsNull(parameter.Enum);
                                });
                        });
                });

            Api(swaggerProvider, "Customers", "/customers", api =>
            {
                // 1: POST /customers
                Assert.AreEqual(1, api.Operations.Count);

                Operation(api, "POST", 0, operation =>
                {
                    Assert.AreEqual("Customers_Create", operation.Nickname);
                    Assert.IsNull(operation.Summary);
                    Assert.IsNull(operation.Notes);
                    Assert.AreEqual("integer", operation.Type);
                    Assert.AreEqual("int32", operation.Format);
                    Assert.IsNull(operation.Items);
                    Assert.IsNull(operation.Enum);

                    Assert.AreEqual(1, operation.Parameters.Count);

                    Parameter(operation, "customer", parameter =>
                    {
                        Assert.AreEqual("body", parameter.ParamType);
                        Assert.IsNull(parameter.Description);
                        Assert.AreEqual(true, parameter.Required);
                        Assert.AreEqual("Customer", parameter.Type);
                        Assert.IsNull(parameter.Format);
                        Assert.IsNull(parameter.Items);
                        Assert.IsNull(parameter.Enum);
                    });
                });
            });

            Api(swaggerProvider, "Customers", "/customers/{id}", api =>
            {
                // 1: DELETE /customers/{id}
                Assert.AreEqual(1, api.Operations.Count);

                Operation(api, "PUT", 0, operation =>
                {
                    Assert.AreEqual("Customers_Update", operation.Nickname);
                    Assert.IsNull(operation.Summary);
                    Assert.IsNull(operation.Notes);
                    Assert.AreEqual("void", operation.Type);
                    Assert.IsNull(operation.Format);
                    Assert.IsNull(operation.Items);
                    Assert.IsNull(operation.Enum);

                    Assert.AreEqual(1, operation.Parameters.Count);

                    Parameter(operation, "id", parameter =>
                    {
                        Assert.AreEqual("path", parameter.ParamType);
                        Assert.IsNull(parameter.Description);
                        Assert.AreEqual(true, parameter.Required);
                        Assert.AreEqual("integer", parameter.Type);
                        Assert.AreEqual("int32", parameter.Format);
                        Assert.IsNull(parameter.Items);
                        Assert.IsNull(parameter.Enum);
                    });
                });
            });

            Api(swaggerProvider, "RandomStuff", "/kittens", api =>
            {
                // 1: POST /unicorns
                Assert.AreEqual(1, api.Operations.Count);

                Operation(api, "POST", 0, operation =>
                {
                    Assert.AreEqual("RandomStuff_CreateKitten", operation.Nickname);
                    Assert.IsNull(operation.Summary);
                    Assert.IsNull(operation.Notes);
                    Assert.AreEqual("string", operation.Type);
                    Assert.IsNull(operation.Format);
                    Assert.IsNull(operation.Items);
                    Assert.IsNull(operation.Enum);

                    Assert.AreEqual(1, operation.Parameters.Count);

                    Parameter(operation, "kitten", parameter =>
                    {
                        Assert.AreEqual("body", parameter.ParamType);
                        Assert.IsNull(parameter.Description);
                        Assert.AreEqual(true, parameter.Required);
                        Assert.AreEqual("string", parameter.Type);
                        Assert.IsNull(parameter.Format);
                        Assert.IsNull(parameter.Items);
                        Assert.IsNull(parameter.Enum);
                    });
                });
            });

            Api(swaggerProvider, "RandomStuff", "/unicorns", api =>
                {
                    // 2: POST /unicorns, GET /unicorns
                    Assert.AreEqual(2, api.Operations.Count);

                    Operation(api, "POST", 0, operation =>
                        {
                            Assert.AreEqual("RandomStuff_CreateUnicorn", operation.Nickname);
                            Assert.IsNull(operation.Summary);
                            Assert.IsNull(operation.Notes);
                            Assert.AreEqual("Unicorn", operation.Type);
                            Assert.IsNull(operation.Format);
                            Assert.IsNull(operation.Items);
                            Assert.IsNull(operation.Enum);

                            Assert.AreEqual(1, operation.Parameters.Count);

                            Parameter(operation, "unicorn", parameter =>
                                {
                                    Assert.AreEqual("body", parameter.ParamType);
                                    Assert.IsNull(parameter.Description);
                                    Assert.AreEqual(true, parameter.Required);
                                    Assert.AreEqual("string", parameter.Type);
                                    Assert.IsNull(parameter.Format);
                                    Assert.IsNull(parameter.Items);
                                    Assert.IsNull(parameter.Enum);
                                });
                        });

                    Operation(api, "GET", 0, operation =>
                    {
                        Assert.AreEqual("RandomStuff_GetUnicorns", operation.Nickname);
                        Assert.IsNull(operation.Summary);
                        Assert.IsNull(operation.Notes);
                        Assert.AreEqual("array", operation.Type);
                        Assert.IsNull(operation.Format);
                        Assert.AreEqual("Unicorn", operation.Items.Ref);
                        Assert.IsNull(operation.Enum);

                        Assert.AreEqual(1, operation.Parameters.Count);

                        Parameter(operation, "propertyValues", parameter =>
                        {
                            Assert.AreEqual("body", parameter.ParamType);
                            Assert.IsNull(parameter.Description);
                            Assert.AreEqual(true, parameter.Required);
                            Assert.AreEqual("array", parameter.Type);
                            Assert.IsNull(parameter.Format);
                            Assert.AreEqual("KeyValuePair[String,String]", parameter.Items.Ref);
                            Assert.IsNull(parameter.Enum);
                        });
                    });
                });

            Api(swaggerProvider, "RandomStuff", "/unicorns/{id}", api =>
                {
                    // 1: DELETE /kittens
                    Assert.AreEqual(1, api.Operations.Count);
                    Operation(api, "DELETE", 0, operation =>
                    {
                        Assert.AreEqual("RandomStuff_DeleteUnicorn", operation.Nickname);
                        Assert.IsNull(operation.Summary);
                        Assert.IsNull(operation.Notes);
                        Assert.AreEqual("string", operation.Type);
                        Assert.IsNull(operation.Format);
                        Assert.IsNull(operation.Items);
                        Assert.IsNull(operation.Enum);

                        Assert.AreEqual(1, operation.Parameters.Count);

                        Parameter(operation, "id", parameter =>
                        {
                            Assert.AreEqual("path", parameter.ParamType);
                            Assert.IsNull(parameter.Description);
                            Assert.AreEqual(true, parameter.Required);
                            Assert.AreEqual("integer", parameter.Type);
                            Assert.AreEqual("int32", parameter.Format);
                            Assert.IsNull(parameter.Items);
                            Assert.IsNull(parameter.Enum);
                        });
                    });
                });
        }

        [Test]
        public void It_should_honor_the_config_setting_to_ignore_obsolete_actions()
        {
            var swaggerProvider = GetSwaggerProvider(ignoreObsoletetActions: true);

            ApiDeclaration(swaggerProvider, "Products", dec => Assert.AreEqual(1, dec.Apis.Count));
        }

        [Test]
        public void It_should_generate_models_for_complex_types_in_a_declaration()
        {
            var swaggerProvider = GetSwaggerProvider();

            ApiDeclaration(swaggerProvider, "Products", dec =>
            {
                // 1: Product
                Assert.AreEqual(1, dec.Models.Count);

                Model(dec, "Product", model =>
                    {
                        CollectionAssert.AreEqual(new[] {"Name", "Price", "Type"}, model.Required);
                        Assert.AreEqual(4, model.Properties.Count);

                        ModelProperty(model, "Id", property =>
                            {
                                Assert.AreEqual("integer", property.Type);
                                Assert.AreEqual("int32", property.Format);
                                Assert.IsNull(property.Items);
                                Assert.IsNull(property.Enum);
                            });

                        ModelProperty(model, "Name", property =>
                            {
                                Assert.AreEqual("string", property.Type);
                                Assert.IsNull(property.Format);
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
                                CollectionAssert.AreEqual(new[] {"Book", "Album", "Shipping", "Packaging"}, property.Enum);
                            });
                    });
            });

            ApiDeclaration(swaggerProvider, "Customers", dec =>
            {
                // 1: Customer
                Assert.AreEqual(1, dec.Models.Count);

                Model(dec, "Customer", model =>
                {
                    CollectionAssert.AreEqual(new[] { "Name", "BirthDate" }, model.Required);
                    Assert.AreEqual(4, model.Properties.Count);

                    ModelProperty(model, "Id", property =>
                    {
                        Assert.AreEqual("integer", property.Type);
                        Assert.AreEqual("int32", property.Format);
                        Assert.IsNull(property.Items);
                        Assert.IsNull(property.Enum);
                    });

                    ModelProperty(model, "Name", property =>
                    {
                        Assert.AreEqual("string", property.Type);
                        Assert.IsNull(property.Format);
                        Assert.IsNull(property.Items);
                        Assert.IsNull(property.Enum);
                    });

                    ModelProperty(model, "BirthDate", property =>
                    {
                        Assert.AreEqual("string", property.Type);
                        Assert.AreEqual("date-time", property.Format);
                        Assert.IsNull(property.Items);
                        Assert.IsNull(property.Enum);
                    });

                    ModelProperty(model, "Associates", property =>
                    {
                        Assert.AreEqual("array", property.Type);
                        Assert.IsNull(property.Format);
                        Assert.AreEqual("Customer", property.Items.Ref);
                        Assert.IsNull(property.Enum);
                    });
                });
            });

            ApiDeclaration(swaggerProvider, "RandomStuff", dec =>
            {
                // 2: Unicorn, Magic<T>, Spells, Spell, KeyValuePair<string, string>
                Assert.AreEqual(5, dec.Models.Count);

                Model(dec, "Unicorn", model =>
                    {
                        CollectionAssert.IsEmpty(model.Required);
                        Assert.AreEqual(2, model.Properties.Count);

                        ModelProperty(model, "MagicName", property =>
                        {
                            Assert.IsNull(property.Type);
                            Assert.AreEqual("Magic[String]", property.Ref);
                            Assert.IsNull(property.Format);
                            Assert.IsNull(property.Enum);
                        });

                        ModelProperty(model, "Spells", property =>
                        {
                            Assert.IsNull(property.Type);
                            Assert.AreEqual("Spells", property.Ref);
                            Assert.IsNull(property.Format);
                            Assert.IsNull(property.Enum);
                        });
                    });

                Model(dec, "Magic[String]", model =>
                    {
                        CollectionAssert.IsEmpty(model.Required);
                        Assert.AreEqual(1, model.Properties.Count);

                        ModelProperty(model, "Normal", property =>
                        {
                            Assert.AreEqual("string", property.Type);
                            Assert.IsNull(property.Format);
                            Assert.IsNull(property.Items);
                            Assert.IsNull(property.Enum);
                        });
                    });

                Model(dec, "Spells", model =>
                    {
                        CollectionAssert.IsEmpty(model.Required);
                        Assert.AreEqual(1, model.Properties.Count);

                        ModelProperty(model, "All", property =>
                        {
                            Assert.AreEqual("array", property.Type);
                            Assert.IsNull(property.Format);
                            Assert.AreEqual("Spell", property.Items.Ref);
                            Assert.IsNull(property.Enum);
                        });
                    });

                Model(dec, "Spell", model =>
                    {
                        CollectionAssert.IsEmpty(model.Required);
                        Assert.AreEqual(1, model.Properties.Count);

                        ModelProperty(model, "Name", property =>
                        {
                            Assert.AreEqual("string", property.Type);
                            Assert.IsNull(property.Format);
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
        }

        [Test]
        public void It_should_override_generation_of_datatype_for_explictly_mapped_types()
        {
            var customMappings = new Dictionary<Type, Func<DataType>> {{typeof (Product), () => new DataType {Type = "string"}}};
            var swaggerProvider = GetSwaggerProvider(customTypeMappings: customMappings);

            ApiDeclaration(swaggerProvider, "Products", dec =>
                {
                    Api(dec, "/products", api =>
                        {
                            Operation(api, "GET", 0, operation =>
                                {
                                    Assert.AreEqual("array", operation.Type);
                                    Assert.IsNull(operation.Format);
                                    Assert.AreEqual("string", operation.Items.Type);
                                    Assert.IsNull(operation.Enum);
                                });

                            Operation(api, "GET", 1, operation =>
                                {
                                    Assert.AreEqual("array", operation.Type);
                                    Assert.IsNull(operation.Format);
                                    Assert.AreEqual("string", operation.Items.Type);
                                    Assert.IsNull(operation.Enum);
                                });
                        });

                    CollectionAssert.IsEmpty(dec.Models);
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
                            ModelProperty(model, "Id", property =>
                                {
                                    Assert.AreEqual("integer", property.Type);
                                    Assert.AreEqual("int32", property.Format);
                                    Assert.IsNull(property.Items);
                                    Assert.IsNull(property.Enum);
                                });

                            ModelProperty(model, "Name", property =>
                            {
                                Assert.AreEqual("string", property.Type);
                                Assert.IsNull(property.Format);
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
                                    CollectionAssert.AreEqual(new[] { "Book", "Album", "Shipping", "Packaging" }, property.Enum);
                                });

                            CollectionAssert.AreEqual(new[] {"Book", "Album", "Service"}, model.SubTypes);
                            Assert.AreEqual("Type", model.Discriminator);
                        });

                    Model(dec, "Book", model =>
                        {
                            Assert.AreEqual(1, model.Properties.Count);

                            ModelProperty(model, "Author", property =>
                                {
                                    Assert.AreEqual("string", property.Type);
                                    Assert.IsNull(property.Format);
                                    Assert.IsNull(property.Items);
                                    Assert.IsNull(property.Enum);
                                });

                            CollectionAssert.IsEmpty(model.SubTypes);
                            Assert.IsNull(model.Discriminator);
                        });

                    Model(dec, "Album", model =>
                        {
                            Assert.AreEqual(1, model.Properties.Count);

                            ModelProperty(model, "Artist", property =>
                                {
                                    Assert.AreEqual("string", property.Type);
                                    Assert.IsNull(property.Format);
                                    Assert.IsNull(property.Items);
                                    Assert.IsNull(property.Enum);
                                });

                            CollectionAssert.IsEmpty(model.SubTypes);
                            Assert.IsNull(model.Discriminator);
                        });

                    Model(dec, "Service", model =>
                        {
                            CollectionAssert.IsEmpty(model.Properties);
                            CollectionAssert.AreEqual(new[] {"Shipping", "Packaging"}, model.SubTypes);
                            Assert.IsNull(model.Discriminator);
                        });

                    Model(dec, "Shipping", model =>
                        {
                            CollectionAssert.IsEmpty(model.Properties);
                            CollectionAssert.IsEmpty(model.SubTypes);
                            Assert.IsNull(model.Discriminator);
                        });

                    Model(dec, "Packaging", model =>
                        {
                            CollectionAssert.IsEmpty(model.Properties);
                            CollectionAssert.IsEmpty(model.SubTypes);
                            Assert.IsNull(model.Discriminator);
                        });
                });
        }

        [Test]
        public void It_should_apply_all_configured_operation_filters()
        {
            var operationFilters = new IOperationFilter[] {new AddStandardResponseCodes(), new AddAuthResponseCodes()};
            var swaggerProvider = GetSwaggerProvider(operationFilters: operationFilters);

            Api(swaggerProvider, "Products", "/products", api =>
                {
                    Operation(api, "GET", 0, operation => Assert.AreEqual(2, operation.ResponseMessages.Count));

                    Operation(api, "GET", 1, operation => Assert.AreEqual(2, operation.ResponseMessages.Count));
                });

            Api(swaggerProvider, "Products", "/products/{id}/suspend", api =>
                Operation(api, "PUT", 0, operation => Assert.AreEqual(2, operation.ResponseMessages.Count)));

            Api(swaggerProvider, "Customers", "/customers", api =>
                Operation(api, "POST", 0, operation => Assert.AreEqual(2, operation.ResponseMessages.Count)));

            Api(swaggerProvider, "Customers", "/customers/{id}", api =>
                Operation(api, "PUT", 0, operation => Assert.AreEqual(2, operation.ResponseMessages.Count)));

            Api(swaggerProvider, "RandomStuff", "/kittens", api =>
                Operation(api, "POST", 0, operation => Assert.AreEqual(2, operation.ResponseMessages.Count)));

            Api(swaggerProvider, "RandomStuff", "/unicorns", api =>
                Operation(api, "POST", 0, operation => Assert.AreEqual(2, operation.ResponseMessages.Count)));

            Api(swaggerProvider, "RandomStuff", "/unicorns/{id}", api =>
                Operation(api, "DELETE", 0, operation => Assert.AreEqual(3, operation.ResponseMessages.Count)));
        }

        private ISwaggerProvider GetSwaggerProvider(
            bool ignoreObsoletetActions = false,
            Func<ApiDescription, string> resourceNameResolver = null,
            Dictionary<Type, Func<DataType>> customTypeMappings = null,
            IEnumerable<PolymorphicType> polymorphicTypes = null,
            IEnumerable<IOperationFilter> operationFilters = null)
        {
            return new ApiExplorerAdapter(
                _apiExplorer,
                ignoreObsoletetActions,
                (apiDesc, version) => true,
                resourceNameResolver ?? (apiDesc => apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName),
                customTypeMappings ?? new Dictionary<Type, Func<DataType>>(), 
                polymorphicTypes ?? new PolymorphicType[]{},
                new List<IModelFilter>(), operationFilters ?? new List<IOperationFilter>());
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