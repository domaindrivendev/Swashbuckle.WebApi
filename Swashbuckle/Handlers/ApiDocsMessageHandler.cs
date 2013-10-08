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
    public class ApiDocsMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            {
                var apiExplorer = request.GetConfiguration().Services.GetApiExplorer();
                var swaggerSpec = SwaggerGenerator.Instance.Generate(apiExplorer);

                var resourceName = request.GetRouteData().Values["resourceName"].ToString();
                if (resourceName == String.Empty)
                    return CreateResponseTask(swaggerSpec.Listing);

                return CreateResponseTask(swaggerSpec.Declarations["/" + resourceName]);
            }
        }

        private Task<HttpResponseMessage> CreateResponseTask(object value)
        {
            var settings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};
            var json = JsonConvert.SerializeObject(value, settings);

            // Create the response.
            var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

            // Note: TaskCompletionSource creates a task that does not contain a delegate.
            var tsc = new TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(response); // Also sets the task state to "RanToCompletion"
            return tsc.Task;
        }
    }
}