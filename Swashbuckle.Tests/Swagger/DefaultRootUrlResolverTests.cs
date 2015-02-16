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
            request.Headers.Add("X-Forwarded-Port", "80");

            var rootUrl = SwaggerDocsConfig.DefaultRootUrlResolver(request);

            Assert.AreEqual("https://acmecorp.org:80", rootUrl);
        }

        private HttpRequestMessage GetRequestFixtureFor(HttpMethod method, string requestUri)
        {
            var fixture = new HttpRequestMessage(method, requestUri);
            fixture.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            return fixture;
        }
    }
}
