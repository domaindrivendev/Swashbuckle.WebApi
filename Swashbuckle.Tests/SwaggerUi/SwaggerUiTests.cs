using System;
using System.Net.Http;
using System.Net;
using NUnit.Framework;
using Swashbuckle.Application;
using Swashbuckle.Dummy;

namespace Swashbuckle.Tests.SwaggerUi
{
    [TestFixture]
    public class SwaggerUiTests : HttpMessageHandlerTestFixture<SwaggerUiHandler>
    {
        private SwaggerUiConfig _swaggerUiConfig;

        public SwaggerUiTests()
            : base("swagger/ui/{*uiPath}")
        { }

        [SetUp]
        public void SetUp()
        {
            var hostNameResolver = Swashbuckle.Configuration.DefaultHostNameResolver();
            _swaggerUiConfig = new SwaggerUiConfig(hostNameResolver, new []{ "swagger/docs/1.0" });

            Handler = new SwaggerUiHandler(_swaggerUiConfig);
        }

        [Test]
        public void It_serves_the_embedded_swagger_ui()
        {
            var content = GetContentAsString("http://tempuri.org/swagger/ui/index.html");

            StringAssert.Contains("discoveryPaths: [ 'swagger/docs/1.0' ]", content);
            StringAssert.Contains("swagger-ui-container", content);
        }
        
        [Test]
        public void It_exposes_config_to_inject_custom_stylesheets()
        {
            var assembly = typeof(SwaggerConfig).Assembly;
            _swaggerUiConfig.InjectStylesheet(assembly, "Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css");
            _swaggerUiConfig.InjectStylesheet(assembly, "Swashbuckle.Dummy.SwaggerExtensions.testStyles2.css");

            var content = GetContentAsString("http://tempuri.org/swagger/ui/index.html");

            StringAssert.Contains(
                "<link href='ext/Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css' media='screen' rel='stylesheet' type='text/css' />\r\n" +
                "<link href='ext/Swashbuckle.Dummy.SwaggerExtensions.testStyles2.css' media='screen' rel='stylesheet' type='text/css' />",
                content);

            content = GetContentAsString("http://tempuri.org/swagger/ui/ext/Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css");
            StringAssert.StartsWith("h1", content);

            content = GetContentAsString("http://tempuri.org/swagger/ui/ext/Swashbuckle.Dummy.SwaggerExtensions.testStyles2.css");
            StringAssert.StartsWith("h2", content);
        }
        
        [Test]
        public void It_exposes_config_for_swagger_ui_settings()
        {
            _swaggerUiConfig
                .SupportHeaderParams()
                .SupportedSubmitMethods(new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Head })
                .DocExpansion(DocExpansion.Full);

            var content = GetContentAsString("http://tempuri.org/swagger/ui/index.html");

            StringAssert.Contains("supportHeaderParams: true", content);
            StringAssert.Contains("supportedSubmitMethods: [ 'get','post','put','head' ]", content);
            StringAssert.Contains("docExpansion: 'full'", content);
        }
        
        [Test]
        public void It_exposes_config_for_swagger_ui_outh2_settings()
        {
            _swaggerUiConfig.EnableOAuth2Support("test-client-id", "test-realm", "Swagger UI");

            var content = GetContentAsString("http://tempuri.org/swagger/ui/index.html");

            StringAssert.Contains("oAuth2Enabled: true", content);
            StringAssert.Contains("oAuth2ClientId: 'test-client-id'", content);
            StringAssert.Contains("oAuth2Realm: 'test-realm'", content);
            StringAssert.Contains("oAuth2AppName: 'Swagger UI'", content);
        }

        [Test]
        public void It_exposes_config_to_enable_a_discovery_url_selector()
        {
            _swaggerUiConfig.EnableDiscoveryUrlSelector();

            var content = GetContentAsString("http://tempuri.org/swagger/ui/index.html");

            StringAssert.Contains("Swashbuckle.SwaggerExtensions.discoveryUrlSelector.js", content);
        }
        

        [Test]
        public void It_exposes_config_to_inject_custom_javascripts()
        {
            var assembly = typeof(SwaggerConfig).Assembly;
            _swaggerUiConfig.InjectJavaScript(assembly, "Swashbuckle.Dummy.SwaggerExtensions.testScript1.js");
            _swaggerUiConfig.InjectJavaScript(assembly, "Swashbuckle.Dummy.SwaggerExtensions.testScript2.js");

            var content = GetContentAsString("http://tempuri.org/swagger/ui/index.html");

            StringAssert.Contains(
                "customScripts: [ " +
                "'ext/Swashbuckle.Dummy.SwaggerExtensions.testScript1.js'," +
                "'ext/Swashbuckle.Dummy.SwaggerExtensions.testScript2.js' " +
                "]",
                content);

            content = GetContentAsString("http://tempuri.org/swagger/ui/ext/Swashbuckle.Dummy.SwaggerExtensions.testScript1.js");
            StringAssert.StartsWith("var str1", content);

            content = GetContentAsString("http://tempuri.org/swagger/ui/ext/Swashbuckle.Dummy.SwaggerExtensions.testScript2.js");
            StringAssert.StartsWith("var str2", content);
        }
        
        [Test]
        public void It_exposes_config_to_serve_custom_web_assets()
        {
            var assembly = typeof(SwaggerConfig).Assembly;
            _swaggerUiConfig.CustomWebAsset("index.html", assembly, "Swashbuckle.Dummy.SwaggerExtensions.myIndex.html");

            var content = GetContentAsString("http://tempuri.org/swagger/ui/index.html");

            StringAssert.Contains("My Index", content);
        }
        
        [Test]
        public void It_errors_on_web_assets_not_found_and_returns_status_not_found()
        {
            var response = Get("http://tempuri.org/swagger/ui/ext/foobar");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}