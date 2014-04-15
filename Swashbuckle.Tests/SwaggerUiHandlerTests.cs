using System.Net.Http;
using System.Threading;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using NUnit.Framework;
using Swashbuckle.Application;

namespace Swashbuckle.Tests
{
    [TestFixture]
    public class SwaggerUiHandlerTests
    {
        private SwaggerUiHandler _handler;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            SwaggerUiConfig.Customize(c =>
            {
                c.SupportHeaderParams = true;
                c.SupportedSubmitMethods = new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Head };
                c.DocExpansion = DocExpansion.Full;
                c.InjectJavaScript(GetType().Assembly, "Swashbuckle.Tests.Support.testScript1.js");
                c.InjectJavaScript(GetType().Assembly, "Swashbuckle.Tests.Support.testScript2.js");
                c.InjectStylesheet(GetType().Assembly, "Swashbuckle.Tests.Support.testStyles1.css");
                c.InjectStylesheet(GetType().Assembly, "Swashbuckle.Tests.Support.testStyles2.css");
            });

            _handler = new SwaggerUiHandler();
        }

        [Test]
        public void It_should_customize_the_swagger_ui_index()
        {
            var responseText = ExecuteRequest("index.html");

            Assert.IsTrue(responseText.Contains("supportHeaderParams: true"), "supportHeaderParams not customized");
            Assert.IsTrue(responseText.Contains("supportedSubmitMethods: ['GET','POST','PUT','HEAD']"), "supportedSubmitMethods not customized");
            Assert.IsTrue(responseText.Contains("docExpansion: \"full\""), "docExpansion not customized");
            Assert.IsTrue(responseText.Contains(
                "$.getScript('ext/Swashbuckle.Tests.Support.testScript1.js');\r\n" +
                "$.getScript('ext/Swashbuckle.Tests.Support.testScript2.js');"),
                "CustomScripts not included");
            Assert.IsTrue(responseText.Contains(
                "<link href='ext/Swashbuckle.Tests.Support.testStyles1.css' rel='stylesheet' type='text/css'/>\r\n" +
                "<link href='ext/Swashbuckle.Tests.Support.testStyles2.css' rel='stylesheet' type='text/css'/>"),
                "Stylesheets not included");
        }

        [Test]
        public void It_should_serve_on_complete_scripts()
        {
            var responseText = ExecuteRequest("ext/Swashbuckle.Tests.Support.testScript1.js");
            Assert.IsTrue(responseText.StartsWith("var testVal = '1';"));

            responseText = ExecuteRequest("ext/Swashbuckle.Tests.Support.testScript2.js");
            Assert.IsTrue(responseText.StartsWith("var testVal = '2';"));
        }

        [Test]
        public void It_should_serve_custom_stylesheets()
        {
            var responseText = ExecuteRequest("ext/Swashbuckle.Tests.Support.testStyles1.css");
            Assert.IsTrue(responseText.StartsWith("body {"));

            responseText = ExecuteRequest("ext/Swashbuckle.Tests.Support.testStyles2.css");
            Assert.IsTrue(responseText.StartsWith("h1 {"));
        }

        private string ExecuteRequest(string uiPath)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uiPath);
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(new HttpRoute("swagger/ui/{*uiPath}"), new HttpRouteValueDictionary(new {uiPath = uiPath}));

            var response = new HttpMessageInvoker(_handler)
                .SendAsync(request, new CancellationToken(false))
                .Result;

            return response.Content.ReadAsStringAsync().Result;
        }
    }
}