using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

namespace Swashbuckle.Application
{
    public class SwaggerUiHandler : HttpMessageHandler
    {
        private readonly SwaggerSpecConfig _swaggerSpecConfig;
        private readonly SwaggerUiConfig _swaggerUiConfig;

        public SwaggerUiHandler()
			: this(SwaggerSpecConfig.StaticInstance, SwaggerUiConfig.StaticInstance)
        {
        }

		public SwaggerUiHandler(SwaggerSpecConfig swaggerSpecConfig, SwaggerUiConfig config)
        {
            _swaggerSpecConfig = swaggerSpecConfig;
            _swaggerUiConfig = config;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uiPath = request.GetRouteData().Values["uiPath"].ToString();

            var contentStream = ContentStreamFor(uiPath);

            if (uiPath == "index.html")
            {
                var discoveryUrls = _swaggerSpecConfig.GetDiscoveryUrls(request);
                contentStream = ApplyPlaceholderValuesTo(contentStream, discoveryUrls);
            }

            var content = new StreamContent(contentStream);
            content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeFor(uiPath));

            var tsc = new TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(new HttpResponseMessage() { Content = content });
            return tsc.Task;
        }

        private Stream ContentStreamFor(string uiPath)
        {
            CustomResourceDescriptor customResourceDescriptor;
            _swaggerUiConfig.CustomRoutes.TryGetValue(uiPath, out customResourceDescriptor);

            return (customResourceDescriptor == null)
                ? GetType().Assembly.GetManifestResourceStream(uiPath)
                : GetCustomResourceStream(customResourceDescriptor);
        }

        private Stream GetCustomResourceStream(CustomResourceDescriptor resourceDescriptor)
        {
            var resourceName = resourceDescriptor.ResourceName;
            var stream = resourceDescriptor.ResourceAssembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new FileNotFoundException("Ensure the Build Action is set to \"Embedded Resource\"", resourceName);

            return stream;
        }

        private Stream ApplyPlaceholderValuesTo(Stream contentStream, IEnumerable<string> discoveryUrls)
        {
            var originalText = new StreamReader(contentStream).ReadToEnd();

            var discoveryUrl = discoveryUrls.Last();

            var listOfSubmitMethods = String.Join(",", _swaggerUiConfig.SupportedSubmitMethods
                .Select(sm => String.Format("'{0}'", sm)));

            var scriptIncludes = String.Join("\r\n", _swaggerUiConfig.CustomScriptPaths
                .Select(path => String.Format("$.getScript('{0}');", path)));

            var stylesheetIncludes = String.Join("\r\n", _swaggerUiConfig.CustomStylesheetPaths
                .Select(path => String.Format("<link href='{0}' rel='stylesheet' type='text/css'/>", path)));

            var customizedText = originalText
                .Replace("%(DiscoveryUrl)", String.Format("\"{0}\"", discoveryUrl))
                .Replace("%(SupportHeaderParams)", _swaggerUiConfig.SupportHeaderParams.ToString().ToLower())
                .Replace("%(SupportedSubmitMethods)", String.Format("[{0}]", listOfSubmitMethods))
                .Replace("%(DocExpansion)", String.Format("\"{0}\"", _swaggerUiConfig.DocExpansion.ToString().ToLower()))
                .Replace("%(CustomScripts)", scriptIncludes)
                .Replace("%(CustomStylesheets)", stylesheetIncludes);

            return new MemoryStream(Encoding.UTF8.GetBytes(customizedText));
        }

        private static string MediaTypeFor(string path)
        {
            var extension = path.Split('.').Last();

            switch (extension)
            {
                case "css":
                    return "text/css";
                case "js":
                    return "text/javascript";
                case "gif":
                    return "image/gif";
                case "png":
                    return "image/png";
                default:
                    return "text/html";
            }
        }
    }
}