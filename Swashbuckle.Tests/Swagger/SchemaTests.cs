using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Swashbuckle.Dummy.Controllers;
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

            dynamic type = new ExpandoObject();
            type.format = "int32";
            type.@enum = new[] {2, 4};
            type.type = "integer";
            ((IDictionary<string, object>)type).Add("x-names", new[] { "Publication", "Album" });

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
                            Type = type,
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
            Assert.IsTrue(JToken.DeepEquals(expected, definitions));
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
        public void It_provides_validation_properties_for_metadata_annotated_types() {
            SetUpDefaultRouteFor<MetadataAnnotatedTypesController>();

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var definitions = swagger["definitions"];
            Assert.IsNotNull(definitions);

            var expected = JObject.FromObject(new {
                PaymentWithMetadata = new {
                    required = new string[] { "Amount", "CardNumber", "ExpMonth", "ExpYear" },
                    type = "object",
                    properties = new {
                        Amount = new {
                            format = "double",
                            type = "number",
                        },
                        CardNumber = new {
                            pattern = "^[3-6]?\\d{12,15}$",
                            type = "string"
                        },
                        ExpMonth = new {
                            format = "int32",
                            maximum = 12,
                            minimum = 1,
                            type = "integer",
                        },
                        ExpYear = new {
                            format = "int32",
                            maximum = 99,
                            minimum = 14,
                            type = "integer",
                        },
                        Note = new {
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

            dynamic category = new ExpandoObject();
            category.@enum = new[] {"A", "B"};
            category.type = "string";
            ((IDictionary<string, object>)category).Add("x-values", new[] { 2, 4 });

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
                        Category = category
                    }
                }
            });

            Assert.IsTrue(JToken.DeepEquals(expected, definitions));
        }

        [Test]
        public void It_honors_json_string_enum_converter_configured_globally()
        {
            Configuration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(
                new StringEnumConverter { CamelCaseText = true });
            SetUpDefaultRouteFor<ProductsController>();

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var typeSchema = swagger["definitions"]["Product"]["properties"]["Type"];

            dynamic category = new ExpandoObject();
            category.@enum = new[] { "publication", "album" };
            category.type = "string";
            ((IDictionary<string, object>)category).Add("x-values", new[] { 2, 4 });

            var expected = JObject.FromObject(category);

            Assert.IsTrue(JToken.DeepEquals(expected, typeSchema));
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
                { "x-type-dotnet", "System.Guid" },
                { "x-nullable", false }
            };
            Assert.AreEqual(JObject.FromObject(expectedParameter).ToString(), parameter.ToString());

            var expectedResponse = new Dictionary<string, object>()
            {
                { "format", "guid" },
                { "type", "string" },
                { "x-type-dotnet", "System.Guid" },
                { "x-nullable", false }
            };
            Assert.AreEqual(JObject.FromObject(expectedResponse).ToString(), response.ToString());
        }

        [Test, TestCaseSource(nameof(GetTestCasesForPrimitives))]
        public void It_exposes_config_to_post_modify_schemas_for_primitives(string action, Type dotNetType, string type, string format, string xtypeDotNet, bool xnullable)
        {
            var underlyingDotNetType = Nullable.GetUnderlyingType(dotNetType) ?? dotNetType;
            SetUpCustomRouteFor<PrimitiveTypesController>("PrimitiveTypes/{action}");
            SetUpHandler(c =>
            {
                if (underlyingDotNetType.IsEnum && type == "string")
                {
                    c.DescribeAllEnumsAsStrings();
                }
                c.SchemaFilter<ApplySchemaVendorExtensions>();
                c.ApplyFiltersToAllSchemas();
            });

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var operation = swagger["paths"]["/PrimitiveTypes/" + action]["post"];
            var parameter = operation["parameters"][0];
            JToken response = operation["responses"]["200"]["schema"];

            var method = typeof(PrimitiveTypesController).GetMethod(action);
            Assert.AreEqual(dotNetType, method.GetParameters()[0].ParameterType);
            Assert.AreEqual(dotNetType, method.ReturnType);

            var expectedParameter = new Dictionary<string, object>
            {
                { "name", "value" },
                { "in", "query" },
                { "required", true },
                { "type", type }
            };
            if (format != null)
            {
                expectedParameter.Add("format", format);
            }
            if (underlyingDotNetType.IsEnum)
            {
                AddEnumValuesValue(type, expectedParameter, underlyingDotNetType);
            }
            expectedParameter.Add("x-type-dotnet", xtypeDotNet);
            expectedParameter.Add("x-nullable", xnullable);

            Assert.IsTrue(JToken.DeepEquals(JObject.FromObject(expectedParameter), parameter));

            var expectedResponse = new Dictionary<string, object>();
            if (format != null)
            {
                expectedResponse.Add("format", format);
            }

            expectedResponse.Add("type", type);

            if (underlyingDotNetType.IsEnum)
            {
                AddEnumValuesValue(type, expectedResponse, underlyingDotNetType);
            }

            expectedResponse.Add("x-type-dotnet", xtypeDotNet);
            expectedResponse.Add("x-nullable", xnullable);

            Assert.IsTrue(JToken.DeepEquals(JObject.FromObject(expectedResponse), response));
        }

        private static void AddEnumValuesValue(string type, IDictionary<string, object> expectedParameter, Type underlyingDotNetType)
        {
            if (type == "string")
            {
                expectedParameter.Add("enum", underlyingDotNetType.GetEnumNames());
                expectedParameter.Add("x-values", underlyingDotNetType.GetEnumValues());
            }
            else
            {
                expectedParameter.Add("enum", underlyingDotNetType.GetEnumValues());
                expectedParameter.Add("x-names", underlyingDotNetType.GetEnumNames());
            }
        }

        [Test, TestCaseSource(nameof(GetTestCasesForPrimitiveArrays))]
        public void It_exposes_config_to_post_modify_schemas_for_primitive_arrays(string action, Type dotNetType, string type, string format, string xtypeDotNet, bool xnullable)
        {
            var underlyingDotNetType = Nullable.GetUnderlyingType(dotNetType) ?? dotNetType;
            SetUpCustomRouteFor<PrimitiveArrayTypesController>("PrimitiveArrayTypes/{action}");
            SetUpHandler(c =>
            {
                if (underlyingDotNetType.IsEnum && type == "string")
                {
                    c.DescribeAllEnumsAsStrings();
                }
                c.SchemaFilter<ApplySchemaVendorExtensions>();
                c.ApplyFiltersToAllSchemas();
            });

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var operation = swagger["paths"]["/PrimitiveArrayTypes/" + action]["post"];
            var parameter = operation["parameters"][0];
            var response = operation["responses"]["200"]["schema"];

            var method = typeof(PrimitiveArrayTypesController).GetMethod(action);
            Assert.AreEqual(dotNetType.MakeArrayType(), method.GetParameters()[0].ParameterType);
            Assert.AreEqual(dotNetType.MakeArrayType(), method.ReturnType);

            var expectedParameterItems = new Dictionary<string, object>();
            if (format != null)
            {
                expectedParameterItems.Add("format", format);
            }
            if (underlyingDotNetType.IsEnum)
            {
                AddEnumValuesValue(type, expectedParameterItems, underlyingDotNetType);
            }
            expectedParameterItems.Add("type", type);
            expectedParameterItems.Add("x-type-dotnet", xtypeDotNet);
            expectedParameterItems.Add("x-nullable", xnullable);
            var expectedParameter = (format == "byte") // Special case
                ? JObject.FromObject(new
                {
                    name = "value",
                    @in = "body",
                    required = true,
                    schema = expectedParameterItems
                })
                : JObject.FromObject(new
                {
                    name = "value",
                    @in = "body",
                    required = true,
                    schema = new
                    {
                        type = "array",
                        items = expectedParameterItems
                    }
                });

            Assert.IsTrue(JToken.DeepEquals(expectedParameter, parameter));

            var expectedResponseItems = new Dictionary<string, object>();
            if (format != null)
            {
                expectedResponseItems.Add("format", format);
            }
            if (underlyingDotNetType.IsEnum)
            {
                AddEnumValuesValue(type, expectedResponseItems, underlyingDotNetType);
            }
            expectedResponseItems.Add("type", type);
            expectedResponseItems.Add("x-type-dotnet", xtypeDotNet);
            expectedResponseItems.Add("x-nullable", xnullable);
            var expectedResponse = (format == "byte") // Special case
                ? JObject.FromObject(expectedResponseItems)
                : JObject.FromObject(new
                {
                    type = "array",
                    items = expectedResponseItems
                });

            Assert.IsTrue(JToken.DeepEquals(expectedResponse, response));
        }


        public static IEnumerable<object[]> GetTestCasesForPrimitives()
        {
            yield return new object[] { "EchoByte", typeof(byte), "integer", "int32", "System.Byte", false }; // Special case
            foreach (var testCase in GetTestCases())
            {
                yield return testCase;
            }
        }

        public static IEnumerable<object[]> GetTestCasesForPrimitiveArrays()
        {
            yield return new object[] { "EchoByte", typeof(byte), "string", "byte", "System.Byte[]", true }; // Special case
            foreach (var testCase in GetTestCases())
            {
                yield return testCase;
            }
        }

        public static IEnumerable<object[]> GetTestCases()
        {
            yield return new object[] { "EchoBoolean", typeof(bool), "boolean", null, "System.Boolean", false };
            yield return new object[] { "EchoSByte", typeof(sbyte), "integer", "int32", "System.SByte", false };
            yield return new object[] { "EchoInt16", typeof(short), "integer", "int32", "System.Int16", false };
            yield return new object[] { "EchoUInt16", typeof(ushort), "integer", "int32", "System.UInt16", false };
            yield return new object[] { "EchoInt32", typeof(int), "integer", "int32", "System.Int32", false };
            yield return new object[] { "EchoUInt32", typeof(uint), "integer", "int32", "System.UInt32", false };
            yield return new object[] { "EchoInt64", typeof(long), "integer", "int64", "System.Int64", false };
            yield return new object[] { "EchoUInt64", typeof(ulong), "integer", "int64", "System.UInt64", false };
            yield return new object[] { "EchoSingle", typeof(float), "number", "float", "System.Single", false };
            yield return new object[] { "EchoDouble", typeof(double), "number", "double", "System.Double", false };
            yield return new object[] { "EchoDecimal", typeof(decimal), "number", "double", "System.Decimal", false };
            yield return new object[] { "EchoDateTime", typeof(DateTime), "string", "date-time", "System.DateTime", false };
            yield return new object[] { "EchoDateTimeOffset", typeof(DateTimeOffset), "string", "date-time", "System.DateTimeOffset", false };
            yield return new object[] { "EchoTimeSpan", typeof(TimeSpan), "string", null, "System.TimeSpan", false };
            yield return new object[] { "EchoGuid", typeof(Guid), "string", "uuid", "System.Guid", false };
            yield return new object[] { "EchoEnum", typeof(PrimitiveEnum), "integer", "int32", "Swashbuckle.Dummy.Types.PrimitiveEnum", false };
            yield return new object[] { "EchoEnum", typeof(PrimitiveEnum), "string", null, "Swashbuckle.Dummy.Types.PrimitiveEnum", false };
            yield return new object[] { "EchoChar", typeof(char), "string", null, "System.Char", false };
            yield return new object[] { "EchoNullableBoolean", typeof(bool?), "boolean", null, "System.Boolean", true };
            yield return new object[] { "EchoNullableByte", typeof(byte?), "integer", "int32", "System.Byte", true };
            yield return new object[] { "EchoNullableSByte", typeof(sbyte?), "integer", "int32", "System.SByte", true };
            yield return new object[] { "EchoNullableInt16", typeof(short?), "integer", "int32", "System.Int16", true };
            yield return new object[] { "EchoNullableUInt16", typeof(ushort?), "integer", "int32", "System.UInt16", true };
            yield return new object[] { "EchoNullableInt32", typeof(int?), "integer", "int32", "System.Int32", true };
            yield return new object[] { "EchoNullableUInt32", typeof(uint?), "integer", "int32", "System.UInt32", true };
            yield return new object[] { "EchoNullableInt64", typeof(long?), "integer", "int64", "System.Int64", true };
            yield return new object[] { "EchoNullableUInt64", typeof(ulong?), "integer", "int64", "System.UInt64", true };
            yield return new object[] { "EchoNullableSingle", typeof(float?), "number", "float", "System.Single", true };
            yield return new object[] { "EchoNullableDouble", typeof(double?), "number", "double", "System.Double", true };
            yield return new object[] { "EchoNullableDecimal", typeof(decimal?), "number", "double", "System.Decimal", true };
            yield return new object[] { "EchoNullableDateTime", typeof(DateTime?), "string", "date-time", "System.DateTime", true };
            yield return new object[] { "EchoNullableDateTimeOffset", typeof(DateTimeOffset?), "string", "date-time", "System.DateTimeOffset", true };
            yield return new object[] { "EchoNullableTimeSpan", typeof(TimeSpan?), "string", null, "System.TimeSpan", true };
            yield return new object[] { "EchoNullableGuid", typeof(Guid?), "string", "uuid", "System.Guid", true };
            yield return new object[] { "EchoNullableEnum", typeof(PrimitiveEnum?), "integer", "int32", "Swashbuckle.Dummy.Types.PrimitiveEnum", true };
            yield return new object[] { "EchoNullableEnum", typeof(PrimitiveEnum?), "string", null, "Swashbuckle.Dummy.Types.PrimitiveEnum", true };
            yield return new object[] { "EchoNullableChar", typeof(char?), "string", null, "System.Char", true };
            yield return new object[] { "EchoString", typeof(string), "string", null, "System.String", true };
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
        public void It_handles_recursion_if_called_again_within_a_filter()
        {
            SetUpCustomRouteFor<ProductsController>("PrimitiveTypes/{action}");
            SetUpHandler(c =>
            {
                c.SchemaFilter<RecursiveCallSchemaFilter>();
            });

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
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
