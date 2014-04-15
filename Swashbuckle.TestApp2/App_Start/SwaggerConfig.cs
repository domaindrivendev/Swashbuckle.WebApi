using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Swashbuckle.Application;
using Swashbuckle.TestApp2;
using WebActivatorEx;

[assembly: PostApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace Swashbuckle.TestApp2
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            // NOTE: Don't remove this line, it's required to wire-up the swagger routes 
            Bootstrapper.Init(GlobalConfiguration.Configuration);

            // NOTE: If you want to customize the generated swagger or UI, use SwaggerSpecConfig and/or SwaggerUiConfig here ...
            SwaggerSpecConfig.Customize(c =>
                {
                    c.ResolveTargetVersionUsing(VersionFromApiKey);
                    c.ResolveVersionSupportUsing(VersionSupportByAttribute);
                });
        }

        private static string VersionFromApiKey(HttpRequestMessage request)
        {
            foreach (var param in request.GetQueryNameValuePairs())
            {
                if (param.Key.ToLower() == "api_key")
                    return param.Value;
            }

            return "2.0";
        }

        private static bool VersionSupportByAttribute(ApiDescription apiDesc, string version)
        {
            var attr = apiDesc.ActionDescriptor.GetCustomAttributes<SupportedInVersionsAttribute>().FirstOrDefault();
            if (attr == null) return false;

            return attr.Versions.Contains(version);
        }
    }
}