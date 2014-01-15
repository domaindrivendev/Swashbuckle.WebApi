using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using NUnit.Framework;
using Swashbuckle.Core.Handlers;
using Swashbuckle.Core.Models;

namespace Swashbuckle.Tests
{
    [TestFixture]
    public class SwaggerUiHandlerTests
    {
        private SwaggerUiHandler _swaggerUiHandler;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            SwaggerUiConfig.Customize(c =>
                {
                    c.ApiKey = "TestApiKey";
                    c.ApiKeyName = "TestApiKeyName";
                    c.SupportHeaderParams = true;
                    c.SupportedSubmitMethods = new[] {HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Head};
                    c.DocExpansion = DocExpansion.Full;
                    c.AddOnCompleteScript(GetType().Assembly, "Swashbuckle.Tests.Support.testScript1.js");
                    c.AddOnCompleteScript(GetType().Assembly, "Swashbuckle.Tests.Support.testScript2.js");
                    c.AddStylesheet(GetType().Assembly, "Swashbuckle.Tests.Support.testStyles1.css");
                    c.AddStylesheet(GetType().Assembly, "Swashbuckle.Tests.Support.testStyles2.css");
                });

            _swaggerUiHandler = new SwaggerUiHandler();
        }

        [Test]
        public void It_should_customize_the_swagger_ui_index()
        {
            var responseText = ExecuteRequest("ui/index.html");

            Assert.IsTrue(responseText.Contains("apiKey: \"TestApiKey\""), "apiKey not customized");
            Assert.IsTrue(responseText.Contains("apiKeyName: \"TestApiKeyName\""), "apiKeyName not customized");
            Assert.IsTrue(responseText.Contains("supportHeaderParams: true"), "supportHeaderParams not customized");
            Assert.IsTrue(responseText.Contains("supportedSubmitMethods: ['GET','POST','PUT','HEAD']"),
                "supportedSubmitMethods not customized");
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
            var responseText = ExecuteRequest("ui/ext/Swashbuckle.Tests.Support.testScript1.js");
            Assert.IsTrue(responseText.StartsWith("var testVal = '1';"));

            responseText = ExecuteRequest("ui/ext/Swashbuckle.Tests.Support.testScript2.js");
            Assert.IsTrue(responseText.StartsWith("var testVal = '2';"));
        }

        [Test]
        public void It_should_serve_custom_stylesheets()
        {
            var responseText = ExecuteRequest("ui/ext/Swashbuckle.Tests.Support.testStyles1.css");
            Assert.IsTrue(responseText.StartsWith("body {"));

            responseText = ExecuteRequest("ui/ext/Swashbuckle.Tests.Support.testStyles2.css");
            Assert.IsTrue(responseText.StartsWith("h1 {"));
        }

        private string ExecuteRequest(string path)
        {
            var config = new HttpConfiguration();
            var route = config.Routes.MapHttpRoute("swagger_ui", "swagger/{*path}");
            var routeValues = new HttpRouteValueDictionary {{"path", path}};

            var requestMessage = new HttpRequestMessage();
            requestMessage.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(route, routeValues);

            var messageInvoker = new HttpMessageInvoker(_swaggerUiHandler);
            var responseMessage = messageInvoker.SendAsync(requestMessage, new CancellationToken(false));

            return responseMessage.Result.Content.ReadAsStringAsync().Result;
        }
    }
}