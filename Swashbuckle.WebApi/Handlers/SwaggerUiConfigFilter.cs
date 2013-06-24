using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Swashbuckle.WebApi.Models;

namespace Swashbuckle.WebApi.Handlers
{
    /// <summary>
    /// NOTE: This filter may produce unexpected results when responses are buffered.
    /// It overrides the Write method to replace placeholders in swagger-ui - index.html with values provided through the fluent config interface.
    /// This means that the filtering is applied to chunks instead of the entire content. 
    /// </summary>
    public class SwaggerUiConfigFilter : MemoryStream
    {
        private readonly Stream _ouputStream;
        private readonly SwaggerUiConfig _swaggerUiConfig;

        public SwaggerUiConfigFilter(Stream ouputStream, SwaggerUiConfig swaggerUiConfig)
        {
            _ouputStream = ouputStream;
            _swaggerUiConfig = swaggerUiConfig;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var text = Encoding.UTF8.GetString(buffer);

            var filteredText = text
                .Replace("%(DiscoveryUrl)", "window.location.href.replace('ui/index.html', 'api-docs')")
                .Replace("%(ApiKeyName)", String.Format("\"{0}\"", _swaggerUiConfig.ApiKeyName))
                .Replace("%(ApiKey)", String.Format("\"{0}\"", _swaggerUiConfig.ApiKey))
                .Replace("%(SupportHeaderParams)", _swaggerUiConfig.SupportHeaderParams.ToString().ToLower())
                .Replace("%(SupportedSubmitMethods)", String.Format("[{0}]", _swaggerUiConfig.SupportedSubmitMethods.ToCommaList()))
                .Replace("%(DocExpansion)", String.Format("\"{0}\"", _swaggerUiConfig.DocExpansion.ToString().ToLower()));

            if (!String.IsNullOrEmpty(_swaggerUiConfig.OnCompleteScriptPath))
                filteredText = filteredText.Replace("%(OnCompleteScript)", String.Format("$.getScript('{0}');", _swaggerUiConfig.OnCompleteScriptPath));

            _ouputStream.Write(Encoding.UTF8.GetBytes(filteredText), offset, Encoding.UTF8.GetByteCount(filteredText));
        }
    }

    internal static class Extensions
    {
        public static string ToCommaList(this IEnumerable<HttpMethod> submitMethods)
        {
            return String.Join(",", submitMethods.Select(m => String.Format("'{0}'", m.Method.ToLower())));
        }
    }
}