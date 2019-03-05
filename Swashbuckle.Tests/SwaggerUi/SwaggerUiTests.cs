using System;
using System.Net;
using System.Collections.Generic;
using NUnit.Framework;
using Swashbuckle.Application;
using Swashbuckle.Dummy;

namespace Swashbuckle.Tests.SwaggerUi
{
    [TestFixture]
    public class SwaggerUiTests : HttpMessageHandlerTestBase<SwaggerUiHandler>
    {
        public SwaggerUiTests()
            : base("swagger/ui/{*assetPath}")
        { }

        [SetUp]
        public void SetUp()
        {
            // Default set-up
            SetUpHandler();
        }

        [Test]
        public void It_serves_the_embedded_swagger_ui()
        {
            var content = GetContentAsString("http://tempuri.org/swagger/ui/index");

            StringAssert.Contains("rootUrl: 'http://tempuri.org'", content);
            StringAssert.Contains("discoveryPaths: arrayFrom('swagger/docs/v1')", content);
            StringAssert.Contains("swagger-ui-container", content);
        }

        [Test]
        public void It_exposes_config_to_inject_custom_stylesheets()
        {
            SetUpHandler(c =>
                {
                    var assembly = typeof(SwaggerConfig).Assembly;
                    c.InjectStylesheet(assembly, "Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css");
                    c.InjectStylesheet(assembly, "Swashbuckle.Dummy.SwaggerExtensions.testStyles2.css", "print");
                });


            var content = GetContentAsString("http://tempuri.org/swagger/ui/index");

            StringAssert.Contains(
                "<link href='ext/Swashbuckle-Dummy-SwaggerExtensions-testStyles1-css' media='screen' rel='stylesheet' type='text/css' />\r\n" +
                "<link href='ext/Swashbuckle-Dummy-SwaggerExtensions-testStyles2-css' media='print' rel='stylesheet' type='text/css' />",
                content);

            content = GetContentAsString("http://tempuri.org/swagger/ui/ext/Swashbuckle-Dummy-SwaggerExtensions-testStyles1-css");
            StringAssert.StartsWith("h1", content);

            content = GetContentAsString("http://tempuri.org/swagger/ui/ext/Swashbuckle-Dummy-SwaggerExtensions-testStyles2-css");
            StringAssert.StartsWith("h2", content);
        }

        [Test]
        public void It_exposes_config_to_set_the_page_title()
        {
            string customTitle = string.Format("TEST_TITLE_{0}", Guid.NewGuid());
            SetUpHandler(c => { c.DocumentTitle(customTitle); });

            var content = GetContentAsString("http://tempuri.org/swagger/ui/index");
            StringAssert.Contains(customTitle, content);
        }

        [Test]
        public void It_exposes_config_for_swagger_ui_settings()
        {
            SetUpHandler(c =>
                {
                    c.DocExpansion(DocExpansion.Full);
                    c.SupportedSubmitMethods("GET", "HEAD");
                });

            var content = GetContentAsString("http://tempuri.org/swagger/ui/index");

            StringAssert.Contains("docExpansion: 'full'", content);
            StringAssert.Contains("supportedSubmitMethods: arrayFrom('get|head')", content);
        }

        [Test]
        public void It_exposes_config_for_swagger_ui_outh2_settings()
        {
            SetUpHandler(c =>
                {
                    c.EnableOAuth2Support(
                        "test-client-id",
                        "test-client-secret",
                        "test-realm",
                        "Swagger UI",
                        " ",
                        new Dictionary<string, string> { { "TestHeader", "TestValue" } });
                });

            var content = GetContentAsString("http://tempuri.org/swagger/ui/index");

            StringAssert.Contains("oAuth2Enabled: ('true' == 'true')", content);
            StringAssert.Contains("oAuth2ClientId: 'test-client-id'", content);
            StringAssert.Contains("oAuth2ClientSecret: 'test-client-secret'", content);
            StringAssert.Contains("oAuth2Realm: 'test-realm'", content);
            StringAssert.Contains("oAuth2AppName: 'Swagger UI'", content);
            StringAssert.Contains("oAuth2ScopeSeperator: ' '", content);
            StringAssert.Contains("oAuth2AdditionalQueryStringParams: JSON.parse('{\"TestHeader\":\"TestValue\"}')", content);
        }

        [Test]
        public void It_exposes_config_to_enable_a_discovery_url_selector()
        {
            SetUpHandler(c => c.EnableDiscoveryUrlSelector());

            var content = GetContentAsString("http://tempuri.org/swagger/ui/index");

            StringAssert.Contains("Swashbuckle-SwaggerUi-CustomAssets-discoveryUrlSelector-js", content);
        }

        [Test]
        public void It_exposes_config_to_inject_custom_javascripts()
        {
            SetUpHandler(c =>
                {
                    var assembly = typeof(SwaggerConfig).Assembly;
                    c.InjectJavaScript(assembly, "Swashbuckle.Dummy.SwaggerExtensions.testScript1.js");
                    c.InjectJavaScript(assembly, "Swashbuckle.Dummy.SwaggerExtensions.testScript2.js");
                });

            var content = GetContentAsString("http://tempuri.org/swagger/ui/index");

            StringAssert.Contains(
                "customScripts: " +
                "arrayFrom('ext/Swashbuckle-Dummy-SwaggerExtensions-testScript1-js|" +
                "ext/Swashbuckle-Dummy-SwaggerExtensions-testScript2-js')",
                content);

            content = GetContentAsString("http://tempuri.org/swagger/ui/ext/Swashbuckle-Dummy-SwaggerExtensions-testScript1-js");
            StringAssert.StartsWith("var str1", content);

            content = GetContentAsString("http://tempuri.org/swagger/ui/ext/Swashbuckle-Dummy-SwaggerExtensions-testScript2-js");
            StringAssert.StartsWith("var str2", content);
        }

        [Test]
        public void It_exposes_config_to_serve_custom_assets()
        {
            SetUpHandler(c =>
                {
                    var assembly = typeof(SwaggerConfig).Assembly;
                    c.CustomAsset("index", assembly, "Swashbuckle.Dummy.SwaggerExtensions.myIndex.html");
                });

            var content = GetContentAsString("http://tempuri.org/swagger/ui/index");

            StringAssert.Contains("My Index", content);
        }

        [Test]
        public void It_exposes_config_to_set_validator_url()
        {
            SetUpHandler(c => c.SetValidatorUrl("http://my-validator.url"));

            var content = GetContentAsString("http://tempuri.org/swagger/ui/index");

            StringAssert.Contains("validatorUrl: stringOrNullFrom('http://my-validator.url')", content);
        }

        [Test]
        public void It_exposes_config_to_disable_validator()
        {
            SetUpHandler(c => c.DisableValidator());

            var content = GetContentAsString("http://tempuri.org/swagger/ui/index");

            StringAssert.Contains("validatorUrl: stringOrNullFrom('null')", content);
        }

        [Test]
        public void It_errors_on_asset_not_found_and_returns_status_not_found()
        {
            var response = Get("http://tempuri.org/swagger/ui/ext/foobar");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public void It_exposes_config_for_apiKey_name_and_location()
        {
            SetUpHandler(c =>
                {
                    c.EnableApiKeySupport("myApiKey", "header");
                });

            var content = GetContentAsString("http://tempuri.org/swagger/ui/index");

            StringAssert.Contains("apiKeyName: 'myApiKey'", content);
            StringAssert.Contains("apiKeyIn: 'header'", content);
        }

        [TestCase("http://tempuri.org/swagger/ui/images/favicon-32x32-png", "image/png")]
        [TestCase("http://tempuri.org/swagger/ui/images/favicon-16x16-png", "image/png")]
        [TestCase("http://tempuri.org/swagger/ui/css/typography-css", "text/css")]
        public void It_returns_correct_asset_mime_type(string resourceUri, string expectedMimeType)
        {
            var response = Get(resourceUri);

            System.Diagnostics.Debug.WriteLine(string.Format("[{0}] {1} => {2}", response.StatusCode, resourceUri, response.Content.Headers.ContentType.MediaType));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(expectedMimeType, response.Content.Headers.ContentType.MediaType);
        }

        private void SetUpHandler(Action<SwaggerUiConfig> configure = null)
        {
            var swaggerUiConfig = new SwaggerUiConfig(new[] { "swagger/docs/v1" }, SwaggerDocsConfig.DefaultRootUrlResolver);

            if (configure != null)
                configure(swaggerUiConfig);

            Handler = new SwaggerUiHandler(swaggerUiConfig);
        }
    }
}