using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Swashbuckle.Models;

namespace Swashbuckle.Handlers
{
    public class SwaggerSpecMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var generator = new SwaggerGenerator(
                SwaggerSpecConfig.Instance.BasePathResolver(request).TrimEnd('/'),
                SwaggerSpecConfig.Instance.DeclarationKeySelector,
                SwaggerSpecConfig.Instance.OperationSpecFilters);

            var swaggerSpec = generator.Generate(request.GetConfiguration().Services.GetApiExplorer());

            var resourceName = GetRequestedResourceName(request);
            if (resourceName == null)
                return CreateResponseFor(swaggerSpec.Listing);

            return CreateResponseFor(swaggerSpec.Declarations["/" + resourceName]);
        }

        private string GetRequestedResourceName(HttpRequestMessage request)
        {
            var routeData = request.GetRouteData().Values;
            if (!routeData.ContainsKey("resource"))
                return null;

            var resourceName = routeData["resource"].ToString();
            return (resourceName == String.Empty) ? null : resourceName;
        }

        private Task<HttpResponseMessage> CreateResponseFor(object value)
        {
            var serializerSettings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};
            var jsonString = JsonConvert.SerializeObject(value, serializerSettings);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonString, Encoding.UTF8, "application/json")
                };

            var tcs = new TaskCompletionSource<HttpResponseMessage>();
            tcs.SetResult(response);
            return tcs.Task;
        }
    }
}
