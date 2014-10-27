using System;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using Swashbuckle.Application;
using Swashbuckle.Dummy.Controllers;

namespace Swashbuckle.Tests.SwaggerFilters
{
    [TestFixture]
    public class SecurityTests : HttpMessageHandlerTestFixture<SwaggerDocsHandler>
    {
        private SwaggerDocsConfig _swaggerDocsConfig;

        public SecurityTests()
            : base("swagger/docs/{apiVersion}")
        {
        }

        [SetUp]
        public void SetUp()
        {
            AddDefaultRouteFor<ProtectedResourcesController>();

            _swaggerDocsConfig = new SwaggerDocsConfig();
            _swaggerDocsConfig.SingleApiVersion("1.0", "Test API");

            Func<HttpRequestMessage, string> hostNameResolver = (req) => req.RequestUri.Host + ":" + req.RequestUri.Port;
            Handler = new SwaggerDocsHandler(hostNameResolver, _swaggerDocsConfig);
        }

        [Test]
        public void It_exposes_config_to_define_a_basic_auth_scheme_for_the_api()
        {
            _swaggerDocsConfig.BasicAuth("basic")
                .Description("Basic HTTP Authentication");

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/1.0");
            var securityDefinitions = swagger["securityDefinitions"];
            var expected = JObject.FromObject(new
                {
                    basic = new
                    {
                        type = "basic",
                        description = "Basic HTTP Authentication"
                    }
                });

            Assert.AreEqual(expected.ToString(), securityDefinitions.ToString());
        }

        [Test]
        public void It_exposes_config_to_define_an_api_key_auth_scheme_for_the_api()
        {
            _swaggerDocsConfig.ApiKey("apiKey")
                .Description("API Key Authentication")
                .Name("apiKey")
                .In("header");

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/1.0");
            var securityDefinitions = swagger["securityDefinitions"];
            var expected = JObject.FromObject(new
                {
                    apiKey = new
                    {
                        type = "apiKey",
                        description = "API Key Authentication",
                        name = "apiKey",
                        @in = "header",
                    }
                });

            Assert.AreEqual(expected.ToString(), securityDefinitions.ToString());
        }

        [Test]
        public void It_exposes_config_to_define_oauth2_flows_for_the_api()
        {
            _swaggerDocsConfig.OAuth2("oauth2")
                .Description("OAuth2 Authorization Code Grant")
                .Flow("accessCode")
                .AuthorizationUrl("https://tempuri.org/auth")
                .TokenUrl("https://tempuri.org/token")
                .Scopes(s =>
                {
                    s.Add("read", "Read access to protected resources");
                    s.Add("write", "Write access to protected resources");
                });

            var swagger = GetContent<JObject>("http://tempuri.org/swagger/docs/1.0");
            var securityDefinitions = swagger["securityDefinitions"];
            var expected = JObject.FromObject(new
                {
                    oauth2 = new
                    {
                        type = "oauth2",
                        description = "OAuth2 Authorization Code Grant",
                        flow = "accessCode",
                        authorizationUrl = "https://tempuri.org/auth",
                        tokenUrl = "https://tempuri.org/token",
                        scopes = new
                        {
                            read = "Read access to protected resources",
                            write = "Write access to protected resources"
                        },
                    }
                });

            Assert.AreEqual(expected.ToString(), securityDefinitions.ToString());
        }
    }
}