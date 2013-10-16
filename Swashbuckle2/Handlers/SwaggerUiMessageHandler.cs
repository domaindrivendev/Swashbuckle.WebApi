using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.Models;

namespace Swashbuckle.Handlers
{
    public class SwaggerUiMessageHandler : HttpMessageHandler
    {
        private readonly SwaggerUiConfig _config;
        private readonly Func<HttpRequestMessage, string> _basePathResolver;

        public SwaggerUiMessageHandler(Func<HttpRequestMessage, string> basePathResolver)
        {
            _basePathResolver = basePathResolver;
            _config = SwaggerUiConfig.Instance;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var resourceName = GetRequestedResourceName(request);
            if (resourceName == null)
            {
                var basePath = _basePathResolver(request).TrimEnd('/');
                return CreateRedirectTo(basePath + "/swagger/index.html");
            }

            var resourceDescriptor = GetResourceDescriptor(resourceName);

            var stream = resourceDescriptor.Assembly.GetManifestResourceStream(resourceDescriptor.Name);
            if (stream == null)
                throw new FileNotFoundException(resourceName);

            HttpContent httpContent;
            if (resourceName == "index.html")
            {
                using (var reader = new StreamReader(stream))
                {
                    var template = reader.ReadToEnd();
                    var responseHtml = SubstituteConfigValues(template);
                    httpContent = new StringContent(responseHtml, Encoding.UTF8, "text/html");
                }
            }
            else
            {
                httpContent = new StreamContent(stream);
            }

            return CreateResponseTaskFor(httpContent);
        }

        private string GetRequestedResourceName(HttpRequestMessage request)
        {
            var resource = request.GetRouteData().Values;
            if (!resource.ContainsKey("resource"))
                return null;

            var path = resource["resource"].ToString();
            return (path == String.Empty) ? null : path.Replace("/", ".");
        }

        private ResourceDescriptor GetResourceDescriptor(string resourceName)
        {
            var customResourceDescriptor = _config.CustomScripts
                .Union(_config.CustomStylesheets)
                .SingleOrDefault(resDesc => resDesc.Name == resourceName);

            return customResourceDescriptor ?? new ResourceDescriptor {Name = resourceName, Assembly = GetType().Assembly};
        }

        private string SubstituteConfigValues(string template)
        {
            var stringBuilder = new StringBuilder(template);

            var supportedSubmitMethods =
                String.Join(",", _config.SupportedSubmitMethods.Select(m => String.Format("'{0}'", m.Method.ToLower())))
                    .TrimEnd(',');

            var customScripts =
                String.Join("\r\n", _config.CustomScripts.Select(res => String.Format("$.getScript('{0}');", res.Name)))
                    .TrimEnd('\r', '\n');

            var customStylesheets =
                String.Join("\r\n", _config.CustomStylesheets.Select(res => String.Format("<link href='{0}' rel='stylesheet' type='text/css'/>", res.Name)))
                    .TrimEnd('\r', '\n');

            return stringBuilder
                .Replace("%(DiscoveryUrl)", "window.location.href.replace(/index\\.html.*/, 'api-docs')")
                .Replace("%(ApiKeyName)", String.Format("\"{0}\"", _config.ApiKeyName))
                .Replace("%(ApiKey)", String.Format("\"{0}\"", _config.ApiKey))
                .Replace("%(SupportHeaderParams)", _config.SupportHeaderParams.ToString().ToLower())
                .Replace("%(SupportedSubmitMethods)", String.Format("[{0}]", supportedSubmitMethods))
                .Replace("%(DocExpansion)", String.Format("\"{0}\"", _config.DocExpansion.ToString().ToLower()))
                .Replace("%(CustomScripts)", customScripts)
                .Replace("%(CustomStylesheets)", customStylesheets)
                .ToString();
        }

        private Task<HttpResponseMessage> CreateRedirectTo(string redirectPath)
        {
            var response = new HttpResponseMessage(HttpStatusCode.Moved);
            response.Headers.Location = new Uri(redirectPath);

            var tcs = new TaskCompletionSource<HttpResponseMessage>();
            tcs.SetResult(response);
            return tcs.Task;
        }

        private Task<HttpResponseMessage> CreateResponseTaskFor(HttpContent httpContent)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = httpContent
                };

            var tcs = new TaskCompletionSource<HttpResponseMessage>();
            tcs.SetResult(response);
            return tcs.Task;
        }
    }
}