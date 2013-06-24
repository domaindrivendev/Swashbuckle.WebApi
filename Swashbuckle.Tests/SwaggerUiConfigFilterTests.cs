using System.IO;
using System.Net.Http;
using NUnit.Framework;
using Swashbuckle.WebApi.Handlers;
using Swashbuckle.WebApi.Models;

namespace Swashbuckle.Tests
{
    [TestFixture]
    public class SwaggerUiConfigFilterTests
    {
        private string ExecuteFilter(SwaggerUiConfig config)
        {
            var outputStream = new MemoryStream();
            var filter = new SwaggerUiConfigFilter(outputStream, config);

            var inputStream = typeof (SwaggerUiConfigFilter).Assembly.GetManifestResourceStream("/swagger/ui/index.html");
            inputStream.CopyTo(filter);

            outputStream.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(outputStream))
            {
                return reader.ReadToEnd();
            }
        }

        [Test]
        public void ItShouldApplyDiscoveryUrlScript()
        {
            var filteredText = ExecuteFilter(new SwaggerUiConfig());
            Assert.IsTrue(filteredText.Contains("discoveryUrl: window.location.href.replace('ui/index.html', 'api-docs')"));
        }

        [Test]
        public void ItShouldApplySupportHeaderParams()
        {
            var filteredText = ExecuteFilter(new SwaggerUiConfig {SupportHeaderParams = true});
            Assert.IsTrue(filteredText.Contains("supportHeaderParams: true"));
        }

        [Test]
        public void ItShouldApplyApiKeyName()
        {
            var filteredText = ExecuteFilter(new SwaggerUiConfig { ApiKeyName = "foo" });
            Assert.IsTrue(filteredText.Contains("apiKeyName: \"foo\""));
        }

        [Test]
        public void ItShouldApplyApiKey()
        {
            var filteredText = ExecuteFilter(new SwaggerUiConfig { ApiKey = "bar" });
            Assert.IsTrue(filteredText.Contains("apiKey: \"bar\""));
        }

        [Test]
        public void ItShouldApplySupportedSubmitMethods()
        {
            var submitMethods = new[] {HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Delete, HttpMethod.Head};
            var filteredText = ExecuteFilter(new SwaggerUiConfig {SupportedSubmitMethods = submitMethods});
            Assert.IsTrue(filteredText.Contains("supportedSubmitMethods: ['get','post','put','delete','head']"));
        }

        [Test]
        public void ItShouldApplyDocExpansion()
        {
            var filteredText = ExecuteFilter(new SwaggerUiConfig {DocExpansion = DocExpansionMode.Full});
            Assert.IsTrue(filteredText.Contains("docExpansion: \"full\""));
        }

        [Test]
        public void ItShouldApplyOnCompleteScriptIfProvided()
        {
            var filteredText = ExecuteFilter(new SwaggerUiConfig { OnCompleteScriptPath = null });
            Assert.IsFalse(filteredText.Contains("$.getScript"));

            filteredText = ExecuteFilter(new SwaggerUiConfig {OnCompleteScriptPath = "/swagger-ext/onComplete.js"});
            Assert.IsTrue(filteredText.Contains("$.getScript('/swagger-ext/onComplete.js');"));
        }
    }
}