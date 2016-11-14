namespace Swashbuckle.Tests.Swagger
{
    using System.Net.Http;
    using NUnit.Framework;
    using Swashbuckle.Application;
    using System.Web.Http.Hosting;
    using System.Web.Http;

    [TestFixture]
    public class DefaultRootUrlResolverTests
    {
        [Test]
        public void It_provides_scheme_host_and_port_from_request_uri()
        {
            var request = GetRequestFixtureFor(HttpMethod.Get, "http://tempuri.org:1234");

            var rootUrl = SwaggerDocsConfig.DefaultRootUrlResolver(request);

            Assert.AreEqual("http://tempuri.org:1234", rootUrl);
        }

        [Test]
        public void It_provides_scheme_host_and_port_from_x_forwarded()
        {  
            var request = GetRequestFixtureFor(HttpMethod.Get, "http://tempuri.org:1234");
            request.Headers.Add("X-Forwarded-Proto", "https");
            request.Headers.Add("X-Forwarded-Host", "acmecorp.org");
            request.Headers.Add("X-Forwarded-Port", "8080");

            var rootUrl = SwaggerDocsConfig.DefaultRootUrlResolver(request);

            Assert.AreEqual("https://acmecorp.org:8080", rootUrl);
        }

        [TestCase("http://tempuri.org", "http://tempuri.org")]
        [TestCase("http://tempuri.org:80", "http://tempuri.org")]
        [TestCase("http://tempuri.org:1234", "http://tempuri.org:1234")]
        [TestCase("https://tempuri.org", "https://tempuri.org")]
        [TestCase("https://tempuri.org:443", "https://tempuri.org")]
        [TestCase("https://tempuri.org:1234", "https://tempuri.org:1234")]
        public void It_provides_scheme_and_host_but_omits_default_port_from_request_uri(string requestedUri, string expectedUri)
        {
            var request = GetRequestFixtureFor(HttpMethod.Get, requestedUri);

            var rootUrl = SwaggerDocsConfig.DefaultRootUrlResolver(request);

            Assert.AreEqual(expectedUri, rootUrl);
        }

        private HttpRequestMessage GetRequestFixtureFor(HttpMethod method, string requestUri)
        {
            var fixture = new HttpRequestMessage(method, requestUri);
            fixture.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            return fixture;
        }
    }
}
