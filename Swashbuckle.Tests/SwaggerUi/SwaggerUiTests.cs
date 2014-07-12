using System.Net.Http;
using System.Threading;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using NUnit.Framework;
using Swashbuckle.Application;
using System.Web.Http;

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
            HttpConfiguration = new HttpConfiguration();
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
            var resourceAssembly = GetType().Assembly;
            _swaggerUiConfig.InjectStylesheet(resourceAssembly, "Swashbuckle.Test.ApiFixtures.testStyles1.css");
            _swaggerUiConfig.InjectStylesheet(resourceAssembly, "Swashbuckle.Test.ApiFixtures.testStyles2.css");

            var content = GetAsString("http://tempuri.org/swagger/ui/index.html");

            Assert.IsTrue(content.Contains(
                "<link href='ext/Swashbuckle.Test.ApiFixtures.testStyles1.css' rel='stylesheet' type='text/css'/>\r\n" +
                "<link href='ext/Swashbuckle.Test.ApiFixtures.testStyles2.css' rel='stylesheet' type='text/css'/>"),
                "Custom stylesheets not included");
		}

		[Test]
		public void It_should_support_custom_javascript_injection()
        {
            var resourceAssembly = GetType().Assembly;
            _swaggerUiConfig.InjectJavaScript(resourceAssembly, "Swashbuckle.Test.ApiFixtures.testScript1.js");
            _swaggerUiConfig.InjectJavaScript(resourceAssembly, "Swashbuckle.Test.ApiFixtures.testScript2.js");

            var content = GetAsString("http://tempuri.org/swagger/ui/index.html");

            Assert.IsTrue(content.Contains(
                "$.getScript('ext/Swashbuckle.Test.ApiFixtures.testScript1.js');\r\n" +
                "$.getScript('ext/Swashbuckle.Test.ApiFixtures.testScript2.js');"),
                "Custom javascripts not included");
		}
    }
}