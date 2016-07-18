using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Swashbuckle.Dummy.Controllers;
using Swashbuckle.Application;
using Swashbuckle.Swagger;
using Swashbuckle.Dummy.SwaggerExtensions;
using Swashbuckle.Dummy.Types;

namespace Swashbuckle.Tests.Swagger
{
    [TestFixture]
    public class SchemaTests : SwaggerTestBase
    {
        public SchemaTests()
            : base("swagger/docs/{apiVersion}")
        { }

        [SetUp]
        public void SetUp()
        {
            // Default set-up
            SetUpHandler();
        }

        [Test]
        public void It_provides_definition_schemas_for_complex_types()
        {
            SetUpDefaultRouteFor<ProductsController>();

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new
                {
                    Product = new
                    {
                        type = "object",
                        properties = new
                        {
                            Id = new
                            {
                                format = "int32",
                                type = "integer",
                                readOnly = true
                            },
                            Type = new
                            {
                                format = "int32",
                                @enum = new[] { 2, 4 },
                                type = "integer"
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
        public void It_provides_object_schemas_for_dictionary_types_with_enum_keys()
        {
            SetUpCustomRouteFor<DictionaryTypesController>("term-definitions");

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var schema = swagger["paths"]["/term-definitions"]["get"]["responses"]["200"]["schema"];

            var expected = JObject.FromObject(new
                {
                    type = "object",
                    properties = new
                    {
                        TermA = new
                        {
                            type = "string"
                        },
                        TermB = new
                        {
                            type = "string"
                        }
                    }
                });

            Assert.IsNotNull(schema);
            Assert.AreEqual(expected.ToString(), schema.ToString());
        }

        [Test]
        public void It_provides_validation_properties_for_annotated_types()
        {
            SetUpDefaultRouteFor<AnnotatedTypesController>();
            
            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
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
                                maxLength = 500,
                                minLength = 10,
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

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new
                {
                    Elephant = new
                    {
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

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var definitions = swagger["definitions"];

            var expected = JObject.FromObject(new
                {
                    Lookup = new
                    {
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
        public void It_honors_json_annotated_attributes()
        {
            SetUpDefaultRouteFor<JsonAnnotatedTypesController>();

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            
            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new
                {
                    JsonRequest = new
                    {
                        type = "object",
                        properties = new
                        {
                            foobar = new
                            {
                                type = "string"
                            },
                            Category = new
                            {
                                @enum = new[] { "A", "B" },
                                type = "string"
                            }
                        }
                    }
                });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        [Test]
        public void It_honors_json_string_enum_converter_configured_globally()
        {
            Configuration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(
                new StringEnumConverter { CamelCaseText = true });
            SetUpDefaultRouteFor<ProductsController>();

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var typeSchema = swagger["definitions"]["Product"]["properties"]["Type"];

            var expected = JObject.FromObject(new
                {
                    @enum = new[] { "publication", "album" },
                    type = "string"
                });
            Assert.AreEqual(expected.ToString(), typeSchema.ToString());
        }

        [Test]
        public void It_exposes_config_to_map_a_type_to_an_explicit_schema()
        {
            SetUpDefaultRouteFor<ProductsController>();
            SetUpHandler(c => c.MapType<ProductType>(() => new Schema
                {
                    type = "integer",
                    format = "int32",
                    maximum = 2,
                    minimum = 1
                }));

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var parameter = swagger["paths"]["/products"]["get"]["parameters"][0];

            var expected = JObject.FromObject(new
                {
                    name = "type",
                    @in = "query",
                    required = true,
                    type = "integer",
                    format = "int32",
                    maximum = 2,
                    minimum = 1
                });
            Assert.AreEqual(expected.ToString(), parameter.ToString());
        }

        [Test]
        public void It_exposes_config_to_post_modify_schemas_for_mapped_types()
        {
            SetUpCustomRouteFor<PrimitiveTypesController>("PrimitiveTypes/{action}");
            SetUpHandler(c =>
            {
                c.MapType<Guid>(() => new Schema { type = "string", format = "guid" }); // map format to guid instead of uuid
                c.SchemaFilter<ApplySchemaVendorExtensions>();
                c.ApplyFiltersToAllSchemas();
            });

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var operation = swagger["paths"]["/PrimitiveTypes/EchoGuid"]["post"];
            var parameter = operation["parameters"][0];
            var response = operation["responses"]["200"]["schema"];

            var method = typeof(PrimitiveTypesController).GetMethod("EchoGuid");
            Assert.AreEqual(typeof(Guid), method.GetParameters()[0].ParameterType);
            Assert.AreEqual(typeof(Guid), method.ReturnType);

            var expectedParameter = new Dictionary<string, object>
            {
                { "name", "value" },
                { "in", "query" },
                { "required", true },
                { "type", "string" },
                { "format", "guid" },
                { "x-schema", "foobar" }
            };
            Assert.AreEqual(JObject.FromObject(expectedParameter).ToString(), parameter.ToString());

            var expectedResponse = new Dictionary<string, object>
            {
                { "format", "guid" },
                { "type", "string" },
                { "x-schema", "foobar" }
            };
            Assert.AreEqual(JObject.FromObject(expectedResponse).ToString(), response.ToString());
        }

        [Test]
        public void It_exposes_config_to_post_modify_schemas_for_primitives()
        {
            SetUpCustomRouteFor<PrimitiveTypesController>("PrimitiveTypes/{action}");
            SetUpHandler(c =>
            {
                c.SchemaFilter<ApplySchemaVendorExtensions>();
                c.ApplyFiltersToAllSchemas();
            });

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var operation = swagger["paths"]["/PrimitiveTypes/EchoBoolean"]["post"];
            var parameter = operation["parameters"][0];
            var response = operation["responses"]["200"]["schema"];

            var method = typeof(PrimitiveTypesController).GetMethod("EchoBoolean");
            Assert.AreEqual(typeof(bool), method.GetParameters()[0].ParameterType);
            Assert.AreEqual(typeof(bool), method.ReturnType);

            var expectedParameter = new Dictionary<string, object>
            {
                { "name", "value" },
                { "in", "query" },
                { "required", true },
                { "type", "boolean" },
                { "x-schema", "foobar" }
            };
            Assert.AreEqual(JObject.FromObject(expectedParameter).ToString(), parameter.ToString());

            var expectedResponse = new Dictionary<string, object>
            {
                { "type", "boolean" },
                { "x-schema", "foobar" }
            };
            Assert.AreEqual(JObject.FromObject(expectedResponse).ToString(), response.ToString());
        }

        [Test]
        public void It_exposes_config_to_post_modify_schemas_for_arrays()
        {
            SetUpCustomRouteFor<PrimitiveArrayTypesController>("PrimitiveArrayTypes/{action}");
            SetUpHandler(c =>
            {
                c.SchemaFilter<ApplySchemaVendorExtensions>();
                c.ApplyFiltersToAllSchemas();
            });

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var operation = swagger["paths"]["/PrimitiveArrayTypes/EchoBoolean"]["post"];
            var parameter = operation["parameters"][0];
            var response = operation["responses"]["200"]["schema"];

            var method = typeof(PrimitiveArrayTypesController).GetMethod("EchoBoolean");
            Assert.AreEqual(typeof(bool[]), method.GetParameters()[0].ParameterType);
            Assert.AreEqual(typeof(bool[]), method.ReturnType);

            var expectedItems = new Dictionary<string, object>
            {
                { "type", "boolean" },
                { "x-schema", "foobar" }
            };

            var expectedParameter = new Dictionary<string, object>
            {
                { "name", "value" },
                { "in", "body" },
                { "required", true },
                { "schema", new Dictionary<string, object>
                    {
                        { "type", "array" },
                        { "items", expectedItems },
                        { "x-schema", "foobar" }
                    }
                }
            };
            Assert.AreEqual(JObject.FromObject(expectedParameter).ToString(), parameter.ToString());

            var expectedResponse = new Dictionary<string, object>
            {
                { "type", "array" },
                { "items", expectedItems },
                { "x-schema", "foobar" }
            };
            Assert.AreEqual(JObject.FromObject(expectedResponse).ToString(), response.ToString());
        }

        [Test]
        public void It_exposes_config_to_post_modify_schemas_for_dictionaries()
        {
            SetUpCustomRouteFor<DictionaryTypesController>("term-definitions");
            SetUpHandler(c =>
            {
                c.SchemaFilter<ApplySchemaVendorExtensions>();
                c.ApplyFiltersToAllSchemas();
            });

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var operation = swagger["paths"]["/term-definitions"]["post"];
            var parameter = operation["parameters"][0];
            var response = operation["responses"]["200"]["schema"];

            var method = typeof(DictionaryTypesController).GetMethod("UpdateTermDescriptions");
            Assert.AreEqual(typeof(IDictionary<Term, string>), method.GetParameters()[0].ParameterType);
            Assert.AreEqual(typeof(IDictionary<Term, string>), method.ReturnType);

            var expectedValue = new Dictionary<string, object>
            {
                { "type", "string" },
                { "x-schema", "foobar" }
            };

            var expectedParameter = new Dictionary<string, object>
            {
                { "name", "terms" },
                { "in", "body" },
                { "required", true },
                { "schema", new Dictionary<string, object>
                    {
                        { "type", "object" },
                        {
                            "properties", new Dictionary<string, object>
                            {
                                { "TermA", expectedValue },
                                { "TermB", expectedValue }
                            }
                        },
                        { "x-schema", "foobar" }
                    }
                }
            };
            Assert.AreEqual(JObject.FromObject(expectedParameter).ToString(), parameter.ToString());

            var expectedResponse = new Dictionary<string, object>
            {
                { "type", "object" },
                {
                    "properties", new Dictionary<string, object>
                    {
                        { "TermA", expectedValue },
                        { "TermB", expectedValue }
                    }
                },
                { "x-schema", "foobar" }
            };
            Assert.AreEqual(JObject.FromObject(expectedResponse).ToString(), response.ToString());
        }

        [Test]
        public void It_exposes_config_to_post_modify_schemas_for_objects()
        {
            SetUpCustomRouteFor<PrimitiveTypesController>("PrimitiveTypes/{action}");
            SetUpHandler(c =>
            {
                c.SchemaFilter<ApplySchemaVendorExtensions>();
                c.ApplyFiltersToAllSchemas();
            });

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var definition = swagger["definitions"]["SimpleComposite"];

            var expectedDefinition = new Dictionary<string, object>
            {
                { "type", "object" },
                { "properties", new Dictionary<string, object>
                    {
                        { "Boolean", new Dictionary<string, object>
                            {
                                { "type", "boolean" },
                                { "x-schema", "foobar" }
                            }
                        },
                        { "String", new Dictionary<string, object>
                            {
                                { "type", "string" },
                                { "x-schema", "foobar" }
                            }
                        }
                    }
                },
                { "x-schema", "foobar" }
            };
            Assert.AreEqual(JObject.FromObject(expectedDefinition).ToString(), definition.ToString());
        }

        [Test]
        public void It_exposes_config_to_ignore_all_properties_that_are_obsolete()
        {
            SetUpDefaultRouteFor<ObsoletePropertiesController>();
            SetUpHandler(c => c.IgnoreObsoleteProperties());

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var calendarProps = swagger["definitions"]["Event"]["properties"];
            var expectedProps = JObject.FromObject(new Dictionary<string, object>
                {
                    {
                        "Name", new { type = "string" }
                    }
                });

            Assert.AreEqual(expectedProps.ToString(), calendarProps.ToString());
        }

        [Test]
        public void It_exposes_config_to_workaround_multiple_types_with_the_same_class_name()
        {
            SetUpDefaultRouteFor<ConflictingTypesController>();
            SetUpHandler(c => c.UseFullTypeNameInSchemaIds());

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var defintitions = swagger["definitions"];

            Assert.AreEqual(2, defintitions.Count());
        }

        [Test]
        public void It_exposes_config_to_choose_schema_id()
        {
            SetUpDefaultRouteFor<ProductsController>();
            SetUpHandler(c => c.SchemaId(t => "my custom name"));

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var defintitions = swagger["definitions"];

            Assert.IsNotNull(defintitions["my custom name"]);
        }

        [Test]
        public void It_exposes_config_to_modify_schema_ids()
        {
            SetUpDefaultRouteFor<ConflictingTypesController>();
            // We have to know the default implementation of FriendlyId before we can modify it's output.
            SetUpHandler(c => { c.SchemaId(t => t.FriendlyId(true).Replace("Swashbuckle.Dummy.Controllers.", String.Empty)); });

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var defintitions = swagger["definitions"];

            Assert.IsNotNull(defintitions["Requests.Blog"]);
        }

        [Test]
        public void It_handles_nested_types()
        {
            SetUpDefaultRouteFor<NestedTypesController>();

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");

            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new
                {
                    Order = new
                    {
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

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");

            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new
                {
                    Component = new
                    {
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
                    },
                    // Breaks current swagger-ui
                    //ListOfSelf = new
                    //{
                    //    type = "array",
                    //    items = JObject.Parse("{ $ref: \"ListOfSelf\" }") 
                    //},
                    DictionaryOfSelf = new
                    {
                        type = "object",
                        additionalProperties = JObject.Parse("{ $ref: \"#/definitions/DictionaryOfSelf\" }") 
                    }
                });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        [Test]
        public void It_handles_two_dimensional_arrays()
        {
            SetUpDefaultRouteFor<TwoDimensionalArraysController>();

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var schema = swagger["paths"]["/twodimensionalarrays"]["post"]["parameters"][0]["schema"];

            var expected = JObject.FromObject(new
                {
                    type = "array",
                    items = new 
                    {
                        type = "array",
                        items = new
                        {
                            format = "int32",
                            type = "integer"
                        }
                    }
                });
            Assert.AreEqual(expected.ToString(), schema.ToString());
        }

        [Test]
        public void It_handles_dynamic_types()
        {
            SetUpDefaultRouteFor<DynamicTypesController>();

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new Dictionary<string, object>
                {
                    {
                        "DynamicObjectSubType", new
                        {
                            type = "object",
                            properties = new
                            {
                                Name = new
                                {
                                    type = "string" 
                                } 
                            }
                        }
                    }
                });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        [Test]
        public void It_handles_nullable_types()
        {
            SetUpDefaultRouteFor<NullableTypesController>();

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var definitions = swagger["definitions"];

            var expected = JObject.FromObject(new Dictionary<string, object>
                {
                    {
                        "Contact", new
                        {
                            type = "object",
                            properties = new
                            {
                                Name = new
                                {
                                    type = "string"
                                },
                                Phone = new
                                {
                                    format = "int32",
                                    type = "integer"
                                }
                            }
                        }
                    }
                });
            Assert.AreEqual(expected.ToString(), definitions.ToString());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void It_errors_on_multiple_types_with_the_same_class_name()
        {
            SetUpDefaultRouteFor<ConflictingTypesController>();

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
        }

        [Test]
        public void It_always_marks_path_parameters_as_required()
        {
            SetUpDefaultRouteFor<PathRequiredController>();

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var required = (bool)swagger["paths"]["/pathrequired/{id}"]["get"]["parameters"][0]["required"];

            Assert.IsTrue(required);
        }
    }
}
