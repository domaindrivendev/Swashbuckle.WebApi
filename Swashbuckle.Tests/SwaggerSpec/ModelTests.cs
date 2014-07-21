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
                        id = "Product",
                        type = "object",
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
                        required = new object[] {},
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
                        id = "Elephant",
                        type = "object",
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
                        required = new object[] { },
                        subTypes = new object[] { }
                    },
                    Animal = new
                    {
                        id = "Animal",
                        type = "object",
                        properties = new
                        {
                            Type = new
                            {
                                type = "string"
                            }
                        },
                        required = new object[] { },
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
                        id = "Order",
                        type = "object",
                        properties = new
                        {
                            LineItems = new
                            {
                                type = "array",
                                items = JObject.Parse("{ $ref: \"LineItem\" }")
                            }
                        },
                        required = new object[] {},
                        subTypes = new object[] {}
                    },
                    LineItem = new
                    {
                        id = "LineItem",
                        type = "object",
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
                        required = new object[] {},
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
                        id = "Component",
                        type = "object",
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
                        required = new object[] {},
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

            Get<JObject>("http://tempuri.org/swagger/api-docs/Matrixes");
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
                        id = "Elephant",
                        type = "object",
                        properties = new
                        {
                            TrunkLength = new
                            {
                                type = "integer",
                                format = "int32"
                            }
                        },
                        required = new object[] {},
                        subTypes = new object[] {}
                    },
                    Animal = new
                    {
                        id = "Animal",
                        type = "object",
                        properties = new
                        {
                            Type = new
                            {
                                type = "string"
                            }
                        },
                        required = new object[] {},
                        subTypes = new object[] { "Mamal" },
                        discriminator = "Type"
                    },
                    Mamal = new
                    {
                        id = "Mamal",
                        type = "object",
                        properties = new
                        {
                            HairColor = new
                            {
                                type = "string"
                            }
                        },
                        required = new object[] {},
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

        class OverrideDescription : IModelFilter
        {
            public void Apply(DataType model, DataTypeRegistry dataTypeRegistry, Type type)
            {
                model.Description = "foobar";
            }
        }
    }
}