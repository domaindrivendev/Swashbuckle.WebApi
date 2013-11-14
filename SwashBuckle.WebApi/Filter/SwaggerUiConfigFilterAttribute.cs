using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Swashbuckle.Models;

namespace SwashBuckle.WebApi.Filter
{
    /// <summary>
    /// NOTE: This filter may produce unexpected results when responses are buffered.
    /// It overrides the Write method to replace placeholders in swagger-ui - index.html with values provided through the fluent config interface.
    /// This means that the filtering is applied to chunks instead of the entire content. 
    /// </summary>
    public class SwaggerUiConfigFilterAttribute : ActionFilterAttribute
    {
        private readonly SwaggerUiConfig _swaggerUiConfig;

        public SwaggerUiConfigFilterAttribute(SwaggerUiConfig swaggerUiConfig)
        {
            _swaggerUiConfig = swaggerUiConfig;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {

            var docRequest = actionContext.ControllerContext.RouteData.Values.ContainsKey("index.html");

            var task = actionContext.Response.Content.ReadAsByteArrayAsync();
            var buffer = task.Result;

            var text = Encoding.UTF8.GetString(buffer);

            var filteredText = text
                .Replace("%(DiscoveryUrl)", "window.location.href.replace(/ui\\/index\\.html.*/, 'api-docs')")
                .Replace("%(ApiKeyName)", String.Format("\"{0}\"", _swaggerUiConfig.ApiKeyName))
                .Replace("%(ApiKey)", String.Format("\"{0}\"", _swaggerUiConfig.ApiKey))
                .Replace("%(SupportHeaderParams)", _swaggerUiConfig.SupportHeaderParams.ToString().ToLower())
                .Replace("%(SupportedSubmitMethods)",
                         String.Format("[{0}]", _swaggerUiConfig.SupportedSubmitMethods.ToCommaList()))
                .Replace("%(DocExpansion)", String.Format("\"{0}\"", _swaggerUiConfig.DocExpansion.ToString().ToLower()))
                .Replace("%(CustomScripts)", _swaggerUiConfig.CustomScripts.ToScriptIncludes())
                .Replace("%(CustomStylesheets)", _swaggerUiConfig.CustomStylesheets.ToStylesheetIncludes());


            actionContext.Response.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(filteredText));
        }
    }


    internal static class Extensions
    {
        public static string ToCommaList(this IEnumerable<HttpMethod> submitMethods)
        {
            return String.Join(",", submitMethods.Select(m => String.Format("'{0}'", m.Method.ToLower())));
        }

        public static string ToScriptIncludes(this IEnumerable<InjectedResourceDescriptor> scriptDescriptors)
        {
            return FormatIncludes("$.getScript('{0}');\r\n", scriptDescriptors);
        }

        public static string ToStylesheetIncludes(this IEnumerable<InjectedResourceDescriptor> stylesheetDescriptors)
        {
            return FormatIncludes("<link href='{0}' rel='stylesheet' type='text/css'/>\r\n", stylesheetDescriptors);
        }

        private static string FormatIncludes(string format, IEnumerable<InjectedResourceDescriptor> elementDescriptors)
        {
            var includesBuilder = new StringBuilder();
            foreach (var descriptor in elementDescriptors)
            {
                includesBuilder.AppendFormat(format, descriptor.RelativePath);
            }
            return includesBuilder.ToString();
        }
    }
}
