using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Swashbuckle.Application;
using Swashbuckle.Dummy.Controllers;
using System.Web.Http;
using System.Linq;
using Swashbuckle.Swagger;

namespace Swashbuckle.Tests.SwaggerSpec
{
    [TestFixture]
    public class OAuth2Tests : HttpMessageHandlerTestsBase<SwaggerSpecHandler>
    {
        private SwaggerSpecConfig _swaggerSpecConfig;

        public OAuth2Tests()
            : base("swagger/{apiVersion}/api-docs")
        { }

        [SetUp]
        public void SetUp()
        {
            _swaggerSpecConfig = new SwaggerSpecConfig();
            Handler = new SwaggerSpecHandler(_swaggerSpecConfig);

            SetUpDefaultRouteFor<ProductsController>();
        }

        [Test]
        public void It_should_support_description_of_implicit_grants()
        {
            _swaggerSpecConfig.Authorization("oauth2", new Authorization
                {
                    Type = "oauth2",
                    Scopes = new []
                    {
                        new Scope { ScopeId = "products.read", Description = "Read products" },
                        new Scope { ScopeId = "products.manage", Description = "Manage products" }
                    },
                    GrantTypes = new GrantTypes
                    {
                        ImplicitGrant = new ImplicitGrant
                        {
                            LoginEndpoint = new LoginEndpoint
                            {
                                Url = "http://tempuri.org/oauth2/login",
                            },
                            TokenName = "access_token"
                        }
                    }
                });

            var listing = Get<JObject>("http://tempuri.org/swagger/api-docs");
            var authorizations = listing["authorizations"];

            var expected = JObject.FromObject(
                new
                {
                    oauth2 = new
                    {
                        type = "oauth2",
                        scopes = new[]
                        {
                            new
                            {
                                scope = "products.read",
                                description = "Read products"
                            },
                            new
                            {
                                scope = "products.manage",
                                description = "Manage products"
                            }
                        },
                        grantTypes = new
                        {
                            @implicit = new
                            {
                                loginEndpoint = new
                                {
                                    url = "http://tempuri.org/oauth2/login"
                                },
                                tokenName = "access_token"
                            }
                        }
                    }
                });

            Assert.AreEqual(expected.ToString(), authorizations.ToString());
        }
        
        [Test]
        public void It_should_support_description_of_authorization_code_grants()
        {
            _swaggerSpecConfig.Authorization("oauth2", new Authorization
                {
                    Type = "oauth2",
                    Scopes = new []
                    {
                        new Scope { ScopeId = "products.read", Description = "Read products" },
                        new Scope { ScopeId = "products.manage", Description = "Manage products" }
                    },
                    GrantTypes = new GrantTypes
                    {
                        AuthorizationCode = new AuthorizationCodeGrant
                        {
                            TokenRequestEndpoint = new TokenRequestEndpoint
                            {
                                Url = "http://tempuri.org/oauth2/authorize",
                                ClientIdName = "client_id",
                                ClientSecretName = "client_secret"
                            },
                            TokenEndpoint = new TokenEndpoint
                            {
                                Url = "http://tempuri.org/oauth2/token",
                                TokenName = "access_token"
                            }
                        }
                    }
                });

            var listing = Get<JObject>("http://tempuri.org/swagger/api-docs");
            var authorizations = listing["authorizations"];

            var expected = JObject.FromObject(
                new
                {
                    oauth2 = new
                    {
                        type = "oauth2",
                        scopes = new[]
                        {
                            new
                            {
                                scope = "products.read",
                                description = "Read products"
                            },
                            new
                            {
                                scope = "products.manage",
                                description = "Manage products"
                            }
                        },
                        grantTypes = new
                        {
                            authorization_code = new
                            {
                                tokenRequestEndpoint = new
                                {
                                    url = "http://tempuri.org/oauth2/authorize",
                                    clientIdName = "client_id",
                                    clientSecretName = "client_secret"
                                },
                                tokenEndpoint = new
                                {
                                    url = "http://tempuri.org/oauth2/token",
                                    tokenName = "access_token"
                                }
                            }
                        }
                    }
                });

            Assert.AreEqual(expected.ToString(), authorizations.ToString());
        }
   }
}
