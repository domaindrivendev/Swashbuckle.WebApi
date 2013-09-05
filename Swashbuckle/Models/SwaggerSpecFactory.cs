using System;
using System.Web;
using System.Web.Http;

namespace Swashbuckle.Models
{
    public class SwaggerSpecFactory
    {
        public static ISwaggerSpec GetSpec()
        {
            return new ApiExplorerAdapter(
                GlobalConfiguration.Configuration.Services.GetApiExplorer(),
                SwaggerSpecConfig.Instance.GroupingStrategy,
                SwaggerSpecConfig.Instance.PostFilters,
                () => HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + HttpRuntime.AppDomainAppVirtualPath);
        }
    }
}