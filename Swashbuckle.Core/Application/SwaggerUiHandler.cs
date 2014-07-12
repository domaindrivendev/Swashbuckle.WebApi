using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Swashbuckle.Application
{
    public class SwaggerUiHandler : HttpMessageHandler
    {
        private readonly SwaggerUiConfig _config;

        public SwaggerUiHandler()
			: this(SwaggerUiConfig.StaticInstance)
        {
            _config = SwaggerUiConfig.StaticInstance;
        }

		public SwaggerUiHandler(SwaggerUiConfig config)
        {
            _config = config;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uiPath = request.GetRouteData().Values["uiPath"];

            var responseMessage = UiResourceResponse(uiPath.ToString());

            return Task.Factory.StartNew(() => responseMessage);
        }

        private HttpResponseMessage UiResourceResponse(string uiPath)
        {
            CustomResourceDescriptor customResourceDescriptor;
            _config.CustomRoutes.TryGetValue(uiPath, out customResourceDescriptor);

            var resourceStream = (customResourceDescriptor == null)
                ? GetType().Assembly.GetManifestResourceStream(uiPath)
                : GetCustomResourceStream(customResourceDescriptor);

            HttpContent content = new StreamContent(resourceStream);
            if (uiPath == "index.html")
                content = CustomizeIndexContent(content);

            content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeFor(uiPath));
            return new HttpResponseMessage { Content = content };
        }

        private Stream GetCustomResourceStream(CustomResourceDescriptor resourceDescriptor)
        {
            var resourceName = resourceDescriptor.ResourceName;
            var stream = resourceDescriptor.ResourceAssembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new FileNotFoundException("Ensure the Build Action is set to \"Embedded Resource\"", resourceName);

            return stream;
        }

        private HttpContent CustomizeIndexContent(HttpContent content)
        {
            var originalText = content.ReadAsStringAsync().Result;

            var listOfSubmitMethods = String.Join(",", _config.SupportedSubmitMethods
                .Select(sm => String.Format("'{0}'", sm)));

            var scriptIncludes = String.Join("\r\n", _config.CustomScriptPaths
                .Select(path => String.Format("$.getScript('{0}');", path)));

            var stylesheetIncludes = String.Join("\r\n", _config.CustomStylesheetPaths
                .Select(path => String.Format("<link href='{0}' rel='stylesheet' type='text/css'/>", path)));

            var customizedText = originalText
                .Replace("%(DiscoveryUrl)", "window.location.href.replace(/ui\\/index\\.html.*/, 'api-docs')")
                .Replace("%(SupportHeaderParams)", _config.SupportHeaderParams.ToString().ToLower())
                .Replace("%(SupportedSubmitMethods)", String.Format("[{0}]", listOfSubmitMethods))
                .Replace("%(DocExpansion)", String.Format("\"{0}\"", _config.DocExpansion.ToString().ToLower()))
                .Replace("%(CustomScripts)", scriptIncludes)
                .Replace("%(CustomStylesheets)", stylesheetIncludes);

            return new StringContent(customizedText);
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
                default:
                    return "text/html";
            }
        }
    }
}