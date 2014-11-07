using Newtonsoft.Json.Linq;
using System;
using NUnit.Framework;
using Swashbuckle.Dummy.Controllers;
using System.Collections.Generic;
using System.Net.Http;
using Swashbuckle.Application;
using Swashbuckle.Swagger;
using Swashbuckle.Dummy.SwaggerExtensions;

namespace Swashbuckle.Tests.Swagger
{
    [TestFixture]
    public class SchemaTests : SwaggerTestBase
    {
        public SchemaTests()
            : base("swagger/docs/{apiVersion}")
        { }

        [SetUp]
        public void SetUP()
        {
            // Default set-up
            SetUpHandler();
        }

        [Test]
        public void It_provides_definition_schemas_for_complex_types()
        {
            SetUpDefaultRouteFor<ProductsController>();

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/1.0");

            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new
                {
                    Product = new
                    {
                        required = new string[] { },
                        type = "object",
                        properties = new
                        {
                            Id = new
                            {
                                format = "int32",
                                type = "integer"
                            },
                            Type = new
                            {
                                @enum = new[] { "Book", "Album" },
                                type = "string"
                            },
                            Description = new
                            {
                                type = "string"
                            },
                            UnitPrice = new
                            {
                                format = "double",
                                type = "number"
                            }
                        }
                    }
                });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        [Test]
        public void It_provides_validation_properties_for_annotated_types()
        {
            SetUpDefaultRouteFor<AnnotatedTypesController>();
            
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/1.0");

            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new
                {
                    Payment = new
                    {
                        required = new string[] { "Amount", "CardNumber", "ExpMonth", "ExpYear" },
                        type = "object",
                        properties = new
                        {
                            Amount = new
                            {
                                format = "double",
                                type = "number",
                            },
                            CardNumber = new
                            {
                                pattern = "^[3-6]?\\d{12,15}$",
                                type = "string"
                            },
                            ExpMonth = new
                            {
                                format = "int32",
                                maximum = 12,
                                minimum = 1,
                                type = "integer",
                            },
                            ExpYear = new
                            {
                                format = "int32",
                                maximum = 99,
                                minimum = 14,
                                type = "integer",
                            },
                            Note = new
                            {
                                type = "string"
                            }
                        }
                    }
                });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        [Test]
        public void It_includes_inherited_properties_for_sub_types()
        {
            SetUpDefaultRouteFor<PolymorphicTypesController>();

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/1.0");

            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new
                {
                    Elephant = new
                    {
                        required = new string[] { },
                        type = "object",
                        properties = new
                        {
                            TrunkLength = new
                            {
                                format = "int32",
                                type = "integer"
                            },
                            HairColor = new
                            {
                                type = "string"
                            },
                            Type = new
                            {
                                type = "string"
                            }
                        }
                    },
                    Animal = new
                    {
                        required = new string[] { },
                        type = "object",
                        properties = new
                        {
                            Type = new
                            {
                                type = "string"
                            }
                        }
                    }
                });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        [Test]
        public void It_omits_indexer_properties()
        {
            SetUpDefaultRouteFor<IndexerTypesController>();

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/1.0");

            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new
                {
                    Lookup = new
                    {
                        required = new string[] { },
                        type = "object",
                        properties = new
                        {
                            TotalEntries = new
                            {
                                format = "int32",
                                type = "integer"
                            }
                        }
                    }
                });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        [Test]
        public void It_honors_newtonsoft_serialization_attributes()
        {
            SetUpDefaultRouteFor<NewtonsoftedTypesController>();

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/1.0");
            var model = swagger["definitions"]["NewtonsoftedModel"];

            Assert.IsNull(model["properties"]["IgnoredProperty"], "Expected the IgnoredProperty to be ignored");
            Assert.IsNotNull(model["properties"]["myCustomNamedProperty"], "Expected the CustomNamedProperty to have the custom name");
        }

        [Test]
        public void It_handles_nested_types()
        {
            SetUpDefaultRouteFor<NestedTypesController>();

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/1.0");

            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new
                {
                    Order = new
                    {
                        required = new string[] { },
                        type = "object",
                        properties = new
                        {
                            LineItems = new
                            {
                                type = "array",
                                items = JObject.Parse("{ $ref: \"#/definitions/LineItem\" }")
                            }
                        }
                    },
                    LineItem = new
                    {
                        required = new string[] { },
                        type = "object",
                        properties = new
                        {
                            ProductId = new
                            {
                                format = "int32",
                                type = "integer"
                            },
                            Quantity = new
                            {
                                format = "int32",
                                type = "integer"
                            }
                        }
                    }
                });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        [Test]
        public void It_handles_self_referencing_types()
        {
            SetUpDefaultRouteFor<SelfReferencingTypesController>();

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/1.0");

            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new
                {
                    Component = new
                    {
                        required = new string[] { },
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
                                items = JObject.Parse("{ $ref: \"#/definitions/Component\" }")
                            }
                        }
                    }
                });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        [Test]
        public void It_handles_jagged_container_types()
        {
            SetUpDefaultRouteFor<JaggedContainersController>();

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/1.0");

            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new Dictionary<string, object>
                {
                    {
                        "Int32[]", new
                        {
                            type = "array",
                            items = new
                            {
                                format = "int32",
                                type = "integer"
                            }
                        }
                    },
                    {
                        "Token", new
                        {
                            type = "array",
                            items = JObject.Parse("{ $ref: \"#/definitions/Token\" }")
                        }
                    }
                });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        [Test]
        public void It_handles_dynamic_types()
        {
            SetUpDefaultRouteFor<DynamicTypesController>();

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/1.0");

            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new Dictionary<string, object>
                {
                    {
                        "Object", new
                        {
                            required = new string[]{},
                            type = "object",
                            properties = new Dictionary<string, Schema>()
                        }
                    }
                });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        //[Test]
        //public void It_exposes_config_to_for_explicit_type_to_schema_mapping()
        //{
        //    SetUpDefaultRouteFor<PolymorphicTypesController>();
        //    SetUpHandler(c =>
        //        {
        //            c.MapType<Mamal>(() => new Schema
        //                {
        //                    type = "object",
        //                    properties = new Dictionary<string, Schema>
        //                    {
        //                        { "HairColor", new Schema { type = "string" }
        //                    },
        //                    allOf
        //                })
        //        })




        //}

        [Test]
        public void It_exposes_config_to_post_modify_schemas()
        {
            SetUpDefaultRouteFor<ProductsController>();
            SetUpHandler(c => c.SchemaFilter<ApplySchemaVendorExtensions>());

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/1.0");
            var xProp = swagger["definitions"]["Product"]["x-schema"];

            Assert.IsNotNull(xProp);
            Assert.AreEqual("bar", xProp.ToString());
        }
    }
}