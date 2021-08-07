using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Owin;

namespace Swashbuckle.Tests.Owin
{
    [TestFixture]
    public abstract class BaseInMemoryOwinSwaggerTest
    {
        protected HttpClient _client;
        private IDisposable _server;

        /// <summary>
        /// Generates swagger documentation only for specific controllers.
        /// </summary>
        /// <param name="controllerType"></param>
        protected void UseInMemoryOwinServer(params Type[] controllerType)
        {
            ConfigureInMemoryOwinServer(appBuilder => ConfigureOwinStartup(controllerType, appBuilder));
        }

        protected void UseInMemoryOwinServer(Action<IAppBuilder> owinStartupBuilder)
        {
            ConfigureInMemoryOwinServer(appBuilder => owinStartupBuilder(appBuilder));
        }

        private void ConfigureOwinStartup(Type[] controller, IAppBuilder appBuilder)
        {
            new OwinStartup(controller).Configuration(appBuilder);
        }

        private void ConfigureInMemoryOwinServer(Action<IAppBuilder> appBuilder)
        {
            var testServer = TestServer.Create(appBuilder);
            _client = testServer.HttpClient;
            _server = testServer;
        }

        [OneTimeTearDown]
        public void TeardownFixture()
        {
            if (_server != null) _server.Dispose();
            _server = null;
        }

        [TearDown]
        public void Teardown()
        {
            if (_server != null) _server.Dispose();
            _server = null;
        }

        protected async Task<JObject> GetSwaggerDocs()
        {
            return await GetContent("swagger/docs/v1");
        }

        protected async Task<JObject> GetContent(string uri)
        {
            var response = await _client.GetAsync(uri);
            if (response.IsSuccessStatusCode == false)
                Assert.Fail("Failed request to {0}: Status code: {1} {2}", uri, response.StatusCode.ToString(), response.ReasonPhrase);
            var content = await response.Content.ReadAsAsync<JObject>();
            return content;

        }
    }
}