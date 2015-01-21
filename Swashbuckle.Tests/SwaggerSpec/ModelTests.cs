using System.Linq;
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
    public class ModelTests : HttpMessageHandlerTestsBase<SwaggerSpecHandler>
    {
        private SwaggerSpecConfig _swaggerSpecConfig;

        public ModelTests()
            : base("swagger/api-docs/{resourceName}")
        {}

        [SetUp]
        public void SetUp()
        {
            _swaggerSpecConfig = new SwaggerSpecConfig();
            Handler = new SwaggerSpecHandler(_swaggerSpecConfig);
        }

        [Test]
        public void It_should_include_a_model_for_each_complex_type_in_a_declaration()
        {
            SetUpDefaultRouteFor<ProductsController>();

            var models = Get<JObject>("http://tempuri.org/swagger/api-docs/Products")
                .SelectToken("models");

            var expected = JObject.FromObject(
                new
                {
                    Product = new
                    {
                        type = "object",
                        id = "Product",
                        required = new object[] {},
                        properties = new
                        {
                            Id = new
                            {
                                type = "integer",
                                format = "int32",
                            },
                            Type = new
                            {
                                type = "string",
                                @enum = new[] { "Book", "Album" }
                            },
                            Description = new
                            {
                                type = "string",
                            },
                            UnitPrice = new
                            {
                                type = "number",
                                format = "double"
                            }
                        },
                        subTypes = new object[] {}
                    }
                }
            );

            Assert.AreEqual(expected.ToString(), models.ToString());
        }
        
        [Test]
        public void It_should_include_inherited_properties_for_complex_types()
        {
            SetUpDefaultRouteFor<PolymorphicTypesController>();

            var models = Get<JObject>("http://tempuri.org/swagger/api-docs/PolymorphicTypes")
                .SelectToken("models");

            var expected = JObject.FromObject(
                new
                {
                    Elephant = new
                    {
                        type = "object",
                        id = "Elephant",
                        required = new object[] { },
                        properties = new
                        {
                            TrunkLength = new
                            {
                                type = "integer",
                                format = "int32"
                            },
                            HairColor = new
                            {
                                type = "string"
                            },
                            Type = new
                            {
                                type = "string"
                            }
                        },
                        subTypes = new object[] { }
                    },
                    Animal = new
                    {
                        type = "object",
                        id = "Animal",
                        required = new object[] { },
                        properties = new
                        {
                            Type = new
                            {
                                type = "string"
                            }
                        },
                        subTypes = new object[] { }
                    }
                }
            );

            Assert.AreEqual(expected.ToString(), models.ToString());
        }
        
        [Test]
        public void It_should_handle_nested_types()
        {
            SetUpDefaultRouteFor<NestedTypesController>();

            var models = Get<JObject>("http://tempuri.org/swagger/api-docs/NestedTypes")
                .SelectToken("models");

            var expected = JObject.FromObject(
                new
                {
                    Order = new
                    {
                        type = "object",
                        id = "Order",
                        required = new object[] {},
                        properties = new
                        {
                            LineItems = new
                            {
                                type = "array",
                                items = JObject.Parse("{ $ref: \"LineItem\" }")
                            }
                        },
                        subTypes = new object[] {}
                    },
                    LineItem = new
                    {
                        type = "object",
                        id = "LineItem",
                        required = new object[] {},
                        properties = new
                        {
                            ProductId = new
                            {
                                type = "integer",
                                format = "int32",
                            },
                            Quantity = new
                            {
                                type = "integer",
                                format = "int32",
                            }
                        },
                        subTypes = new object[] {}
                    }
                }
            );

            Assert.AreEqual(expected.ToString(), models.ToString());
        }

        [Test]
        public void It_should_handle_self_referencing_types()
        {
            SetUpDefaultRouteFor<SelfReferencingTypesController>();

            var models = Get<JObject>("http://tempuri.org/swagger/api-docs/SelfReferencingTypes")
                .SelectToken("models");

            var expected = JObject.FromObject(
                new
                {
                    Component = new
                    {
                        type = "object",
                        id = "Component",
                        required = new object[] {},
                        properties = new
                        {
                            Name = new
                            {
                                type = "string"
                            },
                            SubComponents = new
                            {
                                type = "array",
                                items = JObject.Parse("{ $ref: \"Component\" }")
                            }
                        },
                        subTypes = new object[] {}
                    }
                }
            );

            Assert.AreEqual(expected.ToString(), models.ToString());
        }

        [Test]
        public void It_should_not_create_models_for_dynamic_types_treating_them_instead_as_strings()
        {
            SetUpDefaultRouteFor<DynamicTypesController>();

            var models = Get<JObject>("http://tempuri.org/swagger/api-docs/DynamicTypes")
                .SelectToken("models");

            var expected = JObject.FromObject(
                new {}
            );

            Assert.AreEqual(expected.ToString(), models.ToString());
        }

        [Test]
        public void It_should_honor_required_property_data_annotations()
        {
            SetUpDefaultRouteFor<AnnotatedTypesController>();

            var required = Get<JObject>("http://tempuri.org/swagger/api-docs/AnnotatedTypes")
                .SelectToken("models.Payment.required");

            var expected = JArray.FromObject(new[] { "Amount", "CardNumber" });

            Assert.AreEqual(expected.ToString(), required.ToString());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void It_should_honor_the_swagger_spec_and_not_support_containers_of_containers()
        {
            SetUpDefaultRouteFor<UnsupportedTypesController>();

            Get<JObject>("http://tempuri.org/swagger/api-docs/UnsupportedTypes");
        }

        [Test]
        public void It_should_support_collections_of_primitives()
        {
            SetUpDefaultRouteFor<CollectionOfPrimitivesController>();

            Get<JObject>("http://tempuri.org/swagger/api-docs/CollectionOfPrimitives");
        }

        [Test]
        public void It_should_support_explicit_mapping_of_types_to_data_types()
        {
            SetUpDefaultRouteFor<UnsupportedTypesController>();

            _swaggerSpecConfig.MapType<Matrix>(() => new DataType { Type = "string" });

            var models = Get<JObject>("http://tempuri.org/swagger/api-docs/UnsupportedTypes")
                .SelectToken("models");

            var expected = JObject.FromObject(
                new {}
            );

            Assert.AreEqual(expected.ToString(), models.ToString());
        }

        [Test]
        public void It_should_support_explicit_description_of_polymorphic_types()
        {
            SetUpDefaultRouteFor<PolymorphicTypesController>();

            _swaggerSpecConfig.PolymorphicType<Animal>((config) =>
                {
                    config.DiscriminateBy((a) => a.Type);
                    config.SubType<Mamal>((m) =>
                        {
                            m.SubType<Elephant>();
                        });
                });

            var models = Get<JObject>("http://tempuri.org/swagger/api-docs/PolymorphicTypes")
                .SelectToken("models");

            var expected = JObject.FromObject(
                new
                {
                    Elephant = new
                    {
                        type = "object",
                        id = "Elephant",
                        required = new object[] {},
                        properties = new
                        {
                            TrunkLength = new
                            {
                                type = "integer",
                                format = "int32"
                            }
                        },
                        subTypes = new object[] {}
                    },
                    Animal = new
                    {
                        type = "object",
                        id = "Animal",
                        required = new object[] {},
                        properties = new
                        {
                            Type = new
                            {
                                type = "string"
                            }
                        },
                        subTypes = new object[] { "Mamal" },
                        discriminator = "Type"
                    },
                    Mamal = new
                    {
                        type = "object",
                        id = "Mamal",
                        required = new object[] {},
                        properties = new
                        {
                            HairColor = new
                            {
                                type = "string"
                            }
                        },
                        subTypes = new object[] { "Elephant" }
                    }
                }
            );

            Assert.AreEqual(expected.ToString(), models.ToString());
        }

        [Test]
        public void It_should_support_configurable_filters_for_modifying_generated_models()
        {
            SetUpDefaultRouteFor<ProductsController>();
            
            _swaggerSpecConfig.ModelFilter<OverrideDescription>();

            var models = Get<JObject>("http://tempuri.org/swagger/api-docs/Products")
                .SelectToken("models");

            Assert.AreEqual("foobar", models.SelectToken("Product.description").ToString());
        }

        [Test]
        public void It_should_support_ignoring_model_fields_marked_obsolete()
        {
            SetUpDefaultRouteFor<ObsoleteModelFieldsController>();

            var properties = Get<JObject>("http://tempuri.org/swagger/api-docs/ObsoleteModelFields")
                .SelectToken("models.CreateEntityForm.properties");

            Assert.AreEqual(2, properties.Children().Count());
            Assert.IsNotNull(properties["Id"]);
            Assert.IsNotNull(properties["Name"]);

            _swaggerSpecConfig.IgnoreObsoleteModelFields();

            properties = Get<JObject>("http://tempuri.org/swagger/api-docs/ObsoleteModelFields")
                .SelectToken("models.CreateEntityForm.properties");

            Assert.AreEqual(1, properties.Children().Count());
            Assert.IsNull(properties["Id"]);
            Assert.IsNotNull(properties["Name"]);
        }

        class OverrideDescription : IModelFilter
        {
            public void Apply(DataType model, DataTypeRegistry dataTypeRegistry, Type type)
            {
                model.Description = "foobar";
            }
        }
    }
}