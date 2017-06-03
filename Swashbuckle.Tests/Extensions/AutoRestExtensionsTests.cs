using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Swashbuckle.AutoRestExtensions;
using Swashbuckle.Dummy.Controllers;
using Swashbuckle.Dummy.Types;
using Swashbuckle.Tests.Swagger;

namespace Swashbuckle.Tests.Extensions
{
    [TestFixture]
    public class AutoRestExtensionsTests : SwaggerTestBase
    {
        public AutoRestExtensionsTests() :
            base("swagger/docs/{apiVersion}")
        {
        }

        [SetUp]
        public void SetUp()
        {
            SetUpHandler();
        }

        [Test]
        public void It_exposes_config_to_post_modify_info_for_code_generation_settings()
        {
            // See https://github.com/Azure/autorest/blob/master/Documentation/cli.md for all options
            var settings = new
            {
                clientName = "MyProxy",
                addCredentials = true,
                syncMethods = "None",
                internalConstructors = true,
                useDateTimeOffset = false
            };
            SetUpHandler(c =>
            {
                c.DocumentFilter(() => new CodeGenerationSettingsDocumentFilter(settings));
            });

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var info = swagger["info"];

            var expected = new Dictionary<string, object>
            {
                { "version", "v1" },
                { "title", "Test API" },
                { "x-ms-code-generation-settings", JObject.FromObject(settings) }
            };
            Assert.AreEqual(JObject.FromObject(expected).ToString(), info.ToString());
        }

        [TestCase(false, "integer", "int32")]
        [TestCase(true, "integer", "int32")]
        [TestCase(false, "string", null)]
        [TestCase(true, "string", null)]
        public void It_exposes_config_to_post_modify_schemas_for_enum_types(bool modelAsString, string type, string format)
        {
            SetUpCustomRouteFor<PrimitiveTypesController>("PrimitiveTypes/{action}");
            SetUpHandler(c =>
            {
                if (type == "string")
                {
                    c.DescribeAllEnumsAsStrings();
                }
                c.SchemaFilter(() => new EnumTypeSchemaFilter(modelAsString));
                c.ApplyFiltersToAllSchemas();
            });

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var operation = swagger["paths"]["/PrimitiveTypes/EchoEnum"]["post"];
            var parameter = operation["parameters"][0];
            var response = operation["responses"]["200"]["schema"];

            var method = typeof(PrimitiveTypesController).GetMethod("EchoEnum");
            var enumType = typeof(PrimitiveEnum);
            Assert.AreEqual(enumType, method.GetParameters()[0].ParameterType);
            Assert.AreEqual(enumType, method.ReturnType);

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
            expectedParameter.Add("enum", type == "string" ? enumType.GetEnumNames() : enumType.GetEnumValues());
            expectedParameter.Add("x-ms-enum", new
            {
                name = enumType.Name,
                modelAsString = modelAsString
            });
            Assert.AreEqual(JObject.FromObject(expectedParameter).ToString(), parameter.ToString());

            var expectedResponse = new Dictionary<string, object>();
            if (format != null)
            {
                expectedResponse.Add("format", format);
            }
            expectedResponse.Add("enum", type == "string" ? enumType.GetEnumNames() : enumType.GetEnumValues());
            expectedResponse.Add("type", type);
            expectedResponse.Add("x-ms-enum", new
            {
                name = enumType.Name,
                modelAsString = modelAsString
            });
            Assert.AreEqual(JObject.FromObject(expectedResponse).ToString(), response.ToString());
        }

        [TestCase("EchoChar", typeof(char), "string", "char")]
        [TestCase("EchoDecimal", typeof(decimal), "number", "decimal")]
        [TestCase("EchoTimeSpan", typeof(TimeSpan), "string", "duration")]
        public void It_exposes_config_to_post_modify_schemas_for_custom_types(string action, Type dotNetType, string type, string format)
        {
            SetUpCustomRouteFor<PrimitiveTypesController>("PrimitiveTypes/{action}");
            SetUpHandler(c =>
            {
                c.SchemaFilter<TypeFormatSchemaFilter>();
                c.ApplyFiltersToAllSchemas();
            });

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var operation = swagger["paths"]["/PrimitiveTypes/" + action]["post"];
            var parameter = operation["parameters"][0];
            var response = operation["responses"]["200"]["schema"];

            var method = typeof(PrimitiveTypesController).GetMethod(action);
            Assert.AreEqual(dotNetType, method.GetParameters()[0].ParameterType);
            Assert.AreEqual(dotNetType, method.ReturnType);

            var expectedParameter = new Dictionary<string, object>
            {
                { "name", "value" },
                { "in", "query" },
                { "required", true },
                { "type", type },
                { "format", format }
            };
            Assert.AreEqual(JObject.FromObject(expectedParameter).ToString(), parameter.ToString());

            var expectedResponse = new Dictionary<string, object>
            {
                { "format", format },
                { "type", type }
            };
            Assert.AreEqual(JObject.FromObject(expectedResponse).ToString(), response.ToString());
        }

        [TestCase("EchoBoolean", typeof(bool), "boolean", null, false)]
        [TestCase("EchoByte", typeof(byte), "integer", "int32", false)]
        [TestCase("EchoSByte", typeof(sbyte), "integer", "int32", false)]
        [TestCase("EchoInt16", typeof(short), "integer", "int32", false)]
        [TestCase("EchoUInt16", typeof(ushort), "integer", "int32", false)]
        [TestCase("EchoInt32", typeof(int), "integer", "int32", false)]
        [TestCase("EchoUInt32", typeof(uint), "integer", "int32", false)]
        [TestCase("EchoInt64", typeof(long), "integer", "int64", false)]
        [TestCase("EchoUInt64", typeof(ulong), "integer", "int64", false)]
        [TestCase("EchoSingle", typeof(float), "number", "float", false)]
        [TestCase("EchoDouble", typeof(double), "number", "double", false)]
        [TestCase("EchoDecimal", typeof(decimal), "number", "double", false)]
        [TestCase("EchoDateTime", typeof(DateTime), "string", "date-time", false)]
        [TestCase("EchoDateTimeOffset", typeof(DateTimeOffset), "string", "date-time", false)]
        [TestCase("EchoTimeSpan", typeof(TimeSpan), "string", null, false)]
        [TestCase("EchoGuid", typeof(Guid), "string", "uuid", false)]
        [TestCase("EchoEnum", typeof(PrimitiveEnum), "integer", "int32", false)]
        [TestCase("EchoEnum", typeof(PrimitiveEnum), "string", null, false)]
        [TestCase("EchoChar", typeof(char), "string", null, false)]
        [TestCase("EchoNullableBoolean", typeof(bool?), "boolean", null, true)]
        [TestCase("EchoNullableByte", typeof(byte?), "integer", "int32", true)]
        [TestCase("EchoNullableSByte", typeof(sbyte?), "integer", "int32", true)]
        [TestCase("EchoNullableInt16", typeof(short?), "integer", "int32", true)]
        [TestCase("EchoNullableUInt16", typeof(ushort?), "integer", "int32", true)]
        [TestCase("EchoNullableInt32", typeof(int?), "integer", "int32", true)]
        [TestCase("EchoNullableUInt32", typeof(uint?), "integer", "int32", true)]
        [TestCase("EchoNullableInt64", typeof(long?), "integer", "int64", true)]
        [TestCase("EchoNullableUInt64", typeof(ulong?), "integer", "int64", true)]
        [TestCase("EchoNullableSingle", typeof(float?), "number", "float", true)]
        [TestCase("EchoNullableDouble", typeof(double?), "number", "double", true)]
        [TestCase("EchoNullableDecimal", typeof(decimal?), "number", "double", true)]
        [TestCase("EchoNullableDateTime", typeof(DateTime?), "string", "date-time", true)]
        [TestCase("EchoNullableDateTimeOffset", typeof(DateTimeOffset?), "string", "date-time", true)]
        [TestCase("EchoNullableTimeSpan", typeof(TimeSpan?), "string", null, true)]
        [TestCase("EchoNullableGuid", typeof(Guid?), "string", "uuid", true)]
        [TestCase("EchoNullableEnum", typeof(PrimitiveEnum?), "integer", "int32", true)]
        [TestCase("EchoNullableEnum", typeof(PrimitiveEnum?), "string", null, true)]
        [TestCase("EchoNullableChar", typeof(char?), "string", null, true)]
        [TestCase("EchoString", typeof(string), "string", null, true)]
        public void It_exposes_config_to_post_modify_schemas_for_nullable_primitives(string action, Type dotNetType, string type, string format, bool xnullable)
        {
            var underlyingDotNetType = Nullable.GetUnderlyingType(dotNetType) ?? dotNetType;
            SetUpCustomRouteFor<PrimitiveTypesController>("PrimitiveTypes/{action}");
            SetUpHandler(c =>
            {
                if (underlyingDotNetType.IsEnum && type == "string")
                {
                    c.DescribeAllEnumsAsStrings();
                }
                c.SchemaFilter<NullableTypeSchemaFilter>();
                c.ApplyFiltersToAllSchemas();
            });

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var operation = swagger["paths"]["/PrimitiveTypes/" + action]["post"];
            var parameter = operation["parameters"][0];
            var response = operation["responses"]["200"]["schema"];

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
                expectedParameter.Add("enum", type == "string" ? underlyingDotNetType.GetEnumNames() : underlyingDotNetType.GetEnumValues());
            }
            expectedParameter.Add("x-nullable", xnullable);
            Assert.AreEqual(JObject.FromObject(expectedParameter).ToString(), parameter.ToString());

            var expectedResponse = new Dictionary<string, object>();
            if (format != null)
            {
                expectedResponse.Add("format", format);
            }
            if (underlyingDotNetType.IsEnum)
            {
                expectedResponse.Add("enum", type == "string" ? underlyingDotNetType.GetEnumNames() : underlyingDotNetType.GetEnumValues());
            }
            expectedResponse.Add("type", type);
            expectedResponse.Add("x-nullable", xnullable);
            Assert.AreEqual(JObject.FromObject(expectedResponse).ToString(), response.ToString());
        }

        [TestCase("EchoBoolean", typeof(bool), "boolean", null, false)]
        [TestCase("EchoByte", typeof(byte), "string", "byte", true)] // Special case
        [TestCase("EchoSByte", typeof(sbyte), "integer", "int32", false)]
        [TestCase("EchoInt16", typeof(short), "integer", "int32", false)]
        [TestCase("EchoUInt16", typeof(ushort), "integer", "int32", false)]
        [TestCase("EchoInt32", typeof(int), "integer", "int32", false)]
        [TestCase("EchoUInt32", typeof(uint), "integer", "int32", false)]
        [TestCase("EchoInt64", typeof(long), "integer", "int64", false)]
        [TestCase("EchoUInt64", typeof(ulong), "integer", "int64", false)]
        [TestCase("EchoSingle", typeof(float), "number", "float", false)]
        [TestCase("EchoDouble", typeof(double), "number", "double", false)]
        [TestCase("EchoDecimal", typeof(decimal), "number", "double", false)]
        [TestCase("EchoDateTime", typeof(DateTime), "string", "date-time", false)]
        [TestCase("EchoDateTimeOffset", typeof(DateTimeOffset), "string", "date-time", false)]
        [TestCase("EchoTimeSpan", typeof(TimeSpan), "string", null, false)]
        [TestCase("EchoGuid", typeof(Guid), "string", "uuid", false)]
        [TestCase("EchoEnum", typeof(PrimitiveEnum), "integer", "int32", false)]
        [TestCase("EchoEnum", typeof(PrimitiveEnum), "string", null, false)]
        [TestCase("EchoChar", typeof(char), "string", null, false)]
        [TestCase("EchoNullableBoolean", typeof(bool?), "boolean", null, true)]
        [TestCase("EchoNullableByte", typeof(byte?), "integer", "int32", true)]
        [TestCase("EchoNullableSByte", typeof(sbyte?), "integer", "int32", true)]
        [TestCase("EchoNullableInt16", typeof(short?), "integer", "int32", true)]
        [TestCase("EchoNullableUInt16", typeof(ushort?), "integer", "int32", true)]
        [TestCase("EchoNullableInt32", typeof(int?), "integer", "int32", true)]
        [TestCase("EchoNullableUInt32", typeof(uint?), "integer", "int32", true)]
        [TestCase("EchoNullableInt64", typeof(long?), "integer", "int64", true)]
        [TestCase("EchoNullableUInt64", typeof(ulong?), "integer", "int64", true)]
        [TestCase("EchoNullableSingle", typeof(float?), "number", "float", true)]
        [TestCase("EchoNullableDouble", typeof(double?), "number", "double", true)]
        [TestCase("EchoNullableDecimal", typeof(decimal?), "number", "double", true)]
        [TestCase("EchoNullableDateTime", typeof(DateTime?), "string", "date-time", true)]
        [TestCase("EchoNullableDateTimeOffset", typeof(DateTimeOffset?), "string", "date-time", true)]
        [TestCase("EchoNullableTimeSpan", typeof(TimeSpan?), "string", null, true)]
        [TestCase("EchoNullableGuid", typeof(Guid?), "string", "uuid", true)]
        [TestCase("EchoNullableEnum", typeof(PrimitiveEnum?), "integer", "int32", true)]
        [TestCase("EchoNullableEnum", typeof(PrimitiveEnum?), "string", null, true)]
        [TestCase("EchoNullableChar", typeof(char?), "string", null, true)]
        [TestCase("EchoString", typeof(string), "string", null, true)]
        public void It_exposes_config_to_post_modify_schemas_for_nullable_primitive_arrays(string action, Type dotNetType, string type, string format, bool xnullable)
        {
            var underlyingDotNetType = Nullable.GetUnderlyingType(dotNetType) ?? dotNetType;
            SetUpCustomRouteFor<PrimitiveArrayTypesController>("PrimitiveArrayTypes/{action}");
            SetUpHandler(c =>
            {
                if (underlyingDotNetType.IsEnum && type == "string")
                {
                    c.DescribeAllEnumsAsStrings();
                }
                c.SchemaFilter<NullableTypeSchemaFilter>();
                c.ApplyFiltersToAllSchemas();
            });

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var operation = swagger["paths"]["/PrimitiveArrayTypes/" + action]["post"];
            var parameter = operation["parameters"][0];
            var response = operation["responses"]["200"]["schema"];

            var method = typeof(PrimitiveArrayTypesController).GetMethod(action);
            Assert.AreEqual(dotNetType.MakeArrayType(), method.GetParameters()[0].ParameterType);
            Assert.AreEqual(dotNetType.MakeArrayType(), method.ReturnType);

            var expectedItems = new Dictionary<string, object>();
            if (format != null)
            {
                expectedItems.Add("format", format);
            }
            if (underlyingDotNetType.IsEnum)
            {
                expectedItems.Add("enum", type == "string" ? underlyingDotNetType.GetEnumNames() : underlyingDotNetType.GetEnumValues());
            }
            expectedItems.Add("type", type);
            expectedItems.Add("x-nullable", xnullable);

            var expectedParameter = (format == "byte") // Special case
                ? new Dictionary<string, object>
                {
                    { "name", "value" },
                    { "in", "body" },
                    { "required", true },
                    { "schema", expectedItems }
                }
                : new Dictionary<string, object>
                {
                    { "name", "value" },
                    { "in", "body" },
                    { "required", true },
                    { "schema", new Dictionary<string, object>
                        {
                            { "type", "array" },
                            { "items", expectedItems },
                            { "x-nullable", true }
                        }
                    }
                };
            Assert.AreEqual(JObject.FromObject(expectedParameter).ToString(), parameter.ToString());

            var expectedResponse = (format == "byte") // Special case
                ? expectedItems
                : new Dictionary<string, object>
                {
                    { "type", "array" },
                    { "items", expectedItems },
                    { "x-nullable", true }
                };
            Assert.AreEqual(JObject.FromObject(expectedResponse).ToString(), response.ToString());
        }

        [Test]
        public void It_exposes_config_to_post_modify_schemas_for_nonnullable_properties()
        {
            SetUpCustomRouteFor<PrimitiveTypesController>("PrimitiveTypes/{action}");
            SetUpHandler(c =>
            {
                c.SchemaFilter<NullableTypeSchemaFilter>();
                c.SchemaFilter<NonNullableAsRequiredSchemaFilter>();
                c.ApplyFiltersToAllSchemas();
            });

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/v1");
            var definition = swagger["definitions"]["SimpleComposite"];

            var expectedDefinition = new Dictionary<string, object>
            {
                { "required", new[] { "Boolean" } }, // Non-nullable properties
                { "type", "object" },
                { "properties", new Dictionary<string, object>
                    {
                        { "Boolean", new Dictionary<string, object>
                            {
                                { "type", "boolean" },
                                { "x-nullable", false }
                            }
                        },
                        { "String", new Dictionary<string, object>
                            {
                                { "type", "string" },
                                { "x-nullable", true }
                            }
                        }
                    }
                },
                { "x-nullable", true }
            };
            Assert.AreEqual(JObject.FromObject(expectedDefinition).ToString(), definition.ToString());
        }
    }
}
