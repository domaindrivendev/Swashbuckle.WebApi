using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Swashbuckle.Application;
using Swashbuckle.Dummy.Controllers;
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

            HttpConfiguration = new HttpConfiguration();
        }

        [Test]
        public void It_should_include_a_model_for_each_complex_type_in_a_declaration()
        {
            HttpConfiguration.Routes.Include<ProductsController>();

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
            HttpConfiguration.Routes.Include<KittensController>();

			var models = Get<JObject>("http://tempuri.org/swagger/api-docs/Kittens")
				.SelectToken("models");

            var expected = JObject.FromObject(
                new
                {
                    Kitten = new
                    {
                        id = "Kitten",
                        type = "object",
                        properties = new
                        {
                            HasWhiskers = new
                            {
                                type = "boolean",
                            },
                            Id = new
                            {
                                type = "integer",
                                format = "int32",
                            },
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
        public void It_should_flatten_nested_models()
        {
            HttpConfiguration.Routes.Include<OrdersController>();

            var models = Get<JObject>("http://tempuri.org/swagger/api-docs/Orders")
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
                            Id = new
                            {
                                type = "integer",
                                format = "int32",
                            },
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
        public void It_should_handle_self_referential_models()
        {
            HttpConfiguration.Routes.Include<ComponentsController>();

			var models = Get<JObject>("http://tempuri.org/swagger/api-docs/Components")
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
		public void It_should_not_include_models_for_dynamics_treating_them_instead_as_strings()
        {
            HttpConfiguration.Routes.Include<DynamicsController>();

			var models = Get<JObject>("http://tempuri.org/swagger/api-docs/Dynamics")
				.SelectToken("models");

            var expected = JObject.FromObject(
                new {}
            );

            Assert.AreEqual(expected.ToString(), models.ToString());
        }

		[Test]
		public void It_should_honor_data_annotations_for_required_properties()
        {
            HttpConfiguration.Routes.Include<PaymentsController>();

			var required = Get<JObject>("http://tempuri.org/swagger/api-docs/Payments")
				.SelectToken("models.Payment.required");

            var expected = JArray.FromObject(new[] { "Amount", "CardNumber" });

            Assert.AreEqual(expected.ToString(), required.ToString());
        }

		[Test]
		public void It_should_support_explicit_descriptions_of_polymorphic_types()
        {
			HttpConfiguration.Routes.Include<AnimalsController>();

            _swaggerSpecConfig.PolymorphicType<Animal>((config) =>
                {
                    config.DiscriminateBy((b) => b.Type);
                    config.SubType<Kitten>();
                });

			var models = Get<JObject>("http://tempuri.org/swagger/api-docs/Animals")
				.SelectToken("models");

			var expected = JObject.FromObject(
                new
                {
                    Animal = new
					{
						id = "Animal",
						type = "object",
						properties = new
						{
							Id = new
							{
								type = "integer",
								format = "int32"
							},
							Type = new
							{
								type = "string",
							},
						},
                        required = new object[] {},
                        subTypes = new object[] { "Kitten" },
						discriminator = "Type"
					},
					Kitten = new
					{
						id = "Kitten",
						type = "object",
						properties = new
						{
							HasWhiskers = new
							{
								type = "boolean",
							},
						},
                        required = new object[] {},
                        subTypes = new object[] {}
					},
				}
            );

            Assert.AreEqual(expected.ToString(), models.ToString());
        }

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void It_should_honor_the_swagger_spec_and_not_support_containers_of_containers()
        {
			HttpConfiguration.Routes.Include<MatrixesController>();

            Get<JObject>("http://tempuri.org/swagger/api-docs/Matrixes");
		}
    }
}