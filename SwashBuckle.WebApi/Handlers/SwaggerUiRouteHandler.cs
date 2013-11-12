using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Swashbuckle.Models;

namespace SwashBuckle.WebApi.Handler
{
    public class SwaggerUiRouteHandler : DelegatingHandler
    {
        protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var swaggerUiConfig = SwaggerUiConfig.Instance;

            var routePath = request.RequestUri.LocalPath;
            var resourceAssembly = GetType().Assembly;
            var resourceName = routePath;

            InjectedResourceDescriptor injectedResourceDescriptor;
            if (RequestIsForInjectedResource(routePath, swaggerUiConfig, out injectedResourceDescriptor))
            {
                resourceAssembly = injectedResourceDescriptor.ResourceAssembly;
                resourceName = injectedResourceDescriptor.ResourceName;
            }

            var response = request.CreateResponse(HttpStatusCode.OK);
            if (resourceName == "index.html")
            {
                //response.Filter = new SwaggerUiConfigFilter(response.Filter, swaggerUiConfig);
            }

            var tsc = new TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(response);
            return tsc.Task;
        }

        private bool RequestIsForInjectedResource(string routePath, 
            SwaggerUiConfig swaggerUiConfig,
            out InjectedResourceDescriptor injectedResourceDescriptor)
        {
            injectedResourceDescriptor = swaggerUiConfig.CustomScripts
                .Union(swaggerUiConfig.CustomStylesheets)
                .FirstOrDefault(desc => desc.RelativePath == routePath);

            return injectedResourceDescriptor != null;
        }
    }
}
