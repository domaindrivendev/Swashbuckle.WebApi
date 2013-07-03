using System.IO;
using System.Net.Http;
using System.Web.Routing;
using NUnit.Framework;
using Swashbuckle.Tests.Support;
using Swashbuckle.WebApi.Handlers;
using Swashbuckle.WebApi.Models;

namespace Swashbuckle.Tests
{
    [TestFixture]
    public class SwaggerUiRouteHandlerTests
    {
        private SwaggerUiRouteHandler _routeHandler;

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
                });

            _routeHandler = new SwaggerUiRouteHandler();
        }

        [Test]
        public void ItShouldCustomizeTheSwaggerUiIndex()
        {
            var responseText = ExecuteRequest("/swagger/ui/index.html");

            Assert.IsTrue(responseText.Contains("apiKey: \"TestApiKey\""), "apiKey not customized");
            Assert.IsTrue(responseText.Contains("apiKeyName: \"TestApiKeyName\""), "apiKeyName not customized");
            Assert.IsTrue(responseText.Contains("supportHeaderParams: true"), "supportHeaderParams not customized");
            Assert.IsTrue(responseText.Contains("supportedSubmitMethods: ['get','post','put','head']"), "supportedSubmitMethods not customized");
            Assert.IsTrue(responseText.Contains("docExpansion: \"full\""), "docExpansion not customized");
            Assert.IsTrue(responseText.Contains(
                "$.getScript('/swagger/ui/ext/onComplete_1.js');$.getScript('/swagger/ui/ext/onComplete_2.js');"),
                "onComplete not customized");
        }

        [Test]
        public void ItShouldServeOnCompleteScripts()
        {
            var responseText = ExecuteRequest("/swagger/ui/ext/onComplete_1.js");
            Assert.IsTrue(responseText.StartsWith("var testVal = '1';"));

            responseText = ExecuteRequest("/swagger/ui/ext/onComplete_2.js");
            Assert.IsTrue(responseText.StartsWith("var testVal = '2';"));
        }

        private string ExecuteRequest(string requestPath)
        {
            var httpContext = new FakeHttpContext(requestPath);
            var sinkFilter = new MemoryStream();
            httpContext.Response.Filter = sinkFilter;

            var requestContext = new RequestContext {HttpContext = httpContext};
            var httpHandler = _routeHandler.GetHttpHandler(requestContext) as EmbeddedResourceHttpHandler;
            httpHandler.ProcessRequest(httpContext);

            // Simulate output filtering
            var outputStream = httpContext.Response.OutputStream;
            outputStream.Seek(0, SeekOrigin.Begin);
            outputStream.CopyTo(httpContext.Response.Filter);

            sinkFilter.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(sinkFilter))
            {
                return reader.ReadToEnd();
            }
        }
    }
}