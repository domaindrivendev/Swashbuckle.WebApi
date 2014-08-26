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
        private SwaggerSpecConfig _swaggerSpecConfig; 

        public SwaggerUiTests()
            : base("swagger/ui/{*uiPath}")
        {}

        [SetUp]
        public void Setup()
        {
            SetUpDefaultRouteFor<ProductsController>();

            _swaggerSpecConfig = new SwaggerSpecConfig();
            _swaggerUiConfig = new SwaggerUiConfig();
            Handler = new SwaggerUiHandler(_swaggerSpecConfig, _swaggerUiConfig);
        }

        [Test]
        public void It_should_serve_the_embedded_swagger_ui()
        {
            var content = GetAsString("http://tempuri.org/swagger/ui/index.html");

            StringAssert.Contains("swagger-ui-container", content);
        }

        [Test]
        public void It_should_support_configurable_swagger_ui_settings()
        {
            _swaggerUiConfig.SupportHeaderParams = true;
            _swaggerUiConfig.SupportedSubmitMethods = new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Head };
            _swaggerUiConfig.DocExpansion = DocExpansion.Full;

            var content = GetAsString("http://tempuri.org/swagger/ui/index.html");

            StringAssert.Contains("supportHeaderParams: true", content);
            StringAssert.Contains("supportedSubmitMethods: ['get','post','put','head']", content);
            StringAssert.Contains("docExpansion: \"full\"", content);
        }

        [Test]
        public void It_should_support_custom_stylesheet_injection()
        {
            var resourceAssembly = typeof(SwaggerConfig).Assembly;
            _swaggerUiConfig.InjectStylesheet(resourceAssembly, "Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css");
            _swaggerUiConfig.InjectStylesheet(resourceAssembly, "Swashbuckle.Dummy.SwaggerExtensions.testStyles2.css");

            var content = GetAsString("http://tempuri.org/swagger/ui/index.html");

            StringAssert.Contains(
                "<link href='Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css' rel='stylesheet' type='text/css'/>\r\n" +
                "<link href='Swashbuckle.Dummy.SwaggerExtensions.testStyles2.css' rel='stylesheet' type='text/css'/>",
                content);

            content = GetAsString("http://tempuri.org/swagger/ui/Swashbuckle.Dummy.SwaggerExtensions.testStyles1.css");
            StringAssert.StartsWith("h1", content);

            content = GetAsString("http://tempuri.org/swagger/ui/Swashbuckle.Dummy.SwaggerExtensions.testStyles2.css");
            StringAssert.StartsWith("h2", content);
        }

        [Test]
        public void It_should_support_custom_javascript_injection()
        {
            var resourceAssembly = typeof(SwaggerConfig).Assembly;
            _swaggerUiConfig.InjectJavaScript(resourceAssembly, "Swashbuckle.Dummy.SwaggerExtensions.testScript1.js");
            _swaggerUiConfig.InjectJavaScript(resourceAssembly, "Swashbuckle.Dummy.SwaggerExtensions.testScript2.js");

            var content = GetAsString("http://tempuri.org/swagger/ui/index.html");

            StringAssert.Contains(
                "customScripts: [" +
                "'Swashbuckle.Dummy.SwaggerExtensions.testScript1.js'," +
                "'Swashbuckle.Dummy.SwaggerExtensions.testScript2.js'" +
                "]",
                content);

            content = GetAsString("http://tempuri.org/swagger/ui/Swashbuckle.Dummy.SwaggerExtensions.testScript1.js");
            StringAssert.StartsWith("var str1", content);

            content = GetAsString("http://tempuri.org/swagger/ui/Swashbuckle.Dummy.SwaggerExtensions.testScript2.js");
            StringAssert.StartsWith("var str2", content);
        }
        
        [Test]
        public void It_should_support_customized_route_to_resource_mapping()
        {
            var resourceAssembly = typeof(SwaggerConfig).Assembly;
            _swaggerUiConfig.CustomRoute("index.html", resourceAssembly, "Swashbuckle.Dummy.SwaggerExtensions.myIndex.html");

            var content = GetAsString("http://tempuri.org/swagger/ui/index.html");

            StringAssert.Contains("My Index", content);
        }
    }
}
