using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Swashbuckle.Core.Models;

namespace Swashbuckle.Core.Handlers
{
    public class SwaggerSpecHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var resourceData = request.GetRouteData().Values["resource"];

            var responseMessage = (resourceData == null) ? ListingResponse(request) : DeclarationResponse(request, resourceData.ToString());

            return Task.Factory.StartNew(() => responseMessage);
        }

        private static HttpResponseMessage ListingResponse(HttpRequestMessage request)
        {
            var swaggerSpec = SwaggerSpec.GetInstance(request.GetConfiguration().Services.GetApiExplorer(), request);

            return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonTextFor(swaggerSpec.Listing), Encoding.UTF8, "application/json")
                };
        }

        private static HttpResponseMessage DeclarationResponse(HttpRequestMessage request, string resourceName)
        {
            var swaggerSpec = SwaggerSpec.GetInstance(request.GetConfiguration().Services.GetApiExplorer(), request);

            var declaration = swaggerSpec.Declarations["/" + resourceName];
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonTextFor(declaration), Encoding.UTF8, "application/json")
            };
        }

        private static string JsonTextFor(object value)
        {
            var serializer = new JsonSerializer {NullValueHandling = NullValueHandling.Ignore};
            var jsonBuilder = new StringBuilder();
            using (var writer = new StringWriter(jsonBuilder))
            {
                serializer.Serialize(writer, value);
            }

            return jsonBuilder.ToString();
        }
    }
}