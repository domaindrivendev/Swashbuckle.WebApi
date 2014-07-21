using System.Net.Http;
using System.Threading;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using NUnit.Framework;
using Swashbuckle.Application;
using System.Web.Http;
using Swashbuckle.Dummy.Controllers;
using Swashbuckle.Dummy;

namespace Swashbuckle.Tests.SwaggerUi
{
    [TestFixture]
    public class SwaggerUiTests : HttpMessageHandlerTestsBase<SwaggerUiHandler>
    {
        private SwaggerUiConfig _swaggerUiConfig;

        public SwaggerUiTests()
            : base("swagger/ui/{*uiPath}")
        {}

        [SetUp]
        public void Setup()
        {
            _swaggerUiConfig = new SwaggerUiConfig();
            Handler = new SwaggerUiHandler(_swaggerUiConfig);
        }

        [Test]
        public void It_should_serve_the_embedded_swagger_ui()
        {
            var content = GetAsString("http://tempuri.org/swagger/ui/index.html");

            Assert.IsTrue(content.Contains("swagger-ui-container"), "Expected index.html content not found");
        }

        [Test]
        public void It_should_support_configurable_swagger_ui_settings()
        {
            _swaggerUiConfig.SupportHeaderParams = true;
            _swaggerUiConfig.SupportedSubmitMethods = new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Head };
            _swaggerUiConfig.DocExpansion = DocExpansion.Full;

            var content = GetAsString("http://tempuri.org/swagger/ui/index.html");

            Assert.IsTrue(content.Contains("supportHeaderParams: true"), "supportHeaderParams not customized");
            Assert.IsTrue(content.Contains("supportedSubmitMethods: ['GET','POST','PUT','HEAD']"), "supportedSubmitMethods not customized");
            Assert.IsTrue(content.Contains("docExpansion: \"full\""), "docExpansion not customized");
        }

        [Test]
        public void It_should_support_custom_stylesheet_injection()
        {
            var resourceAssembly = typeof(SwaggerConfig).Assembly;
            _swaggerUiConfig.InjectStylesheet(resourceAssembly, "Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css");
            _swaggerUiConfig.InjectStylesheet(resourceAssembly, "Swashbuckle.Dummy.SwaggerExtensions.testStyles2.css");

            var index = GetAsString("http://tempuri.org/swagger/ui/index.html");

            Assert.IsTrue(index.Contains(
                "<link href='ext/Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css' rel='stylesheet' type='text/css'/>\r\n" +
                "<link href='ext/Swashbuckle.Dummy.SwaggerExtensions.testStyles2.css' rel='stylesheet' type='text/css'/>"),
                "Custom stylesheets not included");

            var content = GetAsString("http://tempuri.org/swagger/ui/ext/Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css");
            Assert.IsTrue(content.StartsWith("h1"), "custom stylesheet not served");

            content = GetAsString("http://tempuri.org/swagger/ui/ext/Swashbuckle.Dummy.SwaggerExtensions.testStyles2.css");
            Assert.IsTrue(content.StartsWith("h2"), "custom stylesheet not served");
        }

        [Test]
        public void It_should_support_custom_javascript_injection()
        {
            var resourceAssembly = typeof(SwaggerConfig).Assembly;
            _swaggerUiConfig.InjectJavaScript(resourceAssembly, "Swashbuckle.Dummy.SwaggerExtensions.testScript1.js");
            _swaggerUiConfig.InjectJavaScript(resourceAssembly, "Swashbuckle.Dummy.SwaggerExtensions.testScript2.js");

            var index = GetAsString("http://tempuri.org/swagger/ui/index.html");

            Assert.IsTrue(index.Contains(
                "$.getScript('ext/Swashbuckle.Dummy.SwaggerExtensions.testScript1.js');\r\n" +
                "$.getScript('ext/Swashbuckle.Dummy.SwaggerExtensions.testScript2.js');"),
                "Custom javascripts not included");

            var content = GetAsString("http://tempuri.org/swagger/ui/ext/Swashbuckle.Dummy.SwaggerExtensions.testScript1.js");
            Assert.IsTrue(content.StartsWith("var str1"), "custom javascript not served");

            content = GetAsString("http://tempuri.org/swagger/ui/ext/Swashbuckle.Dummy.SwaggerExtensions.testScript2.js");
            Assert.IsTrue(content.StartsWith("var str2"), "custom javascript not served");
        }
        
        [Test]
        public void It_should_support_customized_route_to_resource_mapping()
        {
            var resourceAssembly = typeof(SwaggerConfig).Assembly;
            _swaggerUiConfig.CustomRoute("index.html", resourceAssembly, "Swashbuckle.Dummy.SwaggerExtensions.myIndex.html");

            var content = GetAsString("http://tempuri.org/swagger/ui/index.html");

            Assert.IsTrue(content.Contains("My Index"), "Custom javascripts not included");
        }
    }
}