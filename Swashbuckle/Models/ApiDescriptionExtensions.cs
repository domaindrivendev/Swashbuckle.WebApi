using System;
using System.Linq;
using System.Web.Http.Description;

namespace Swashbuckle.Models
{
    public static class ApiDescriptionExtensions
    {
        public static string Nickname(this ApiDescription apiDesc)
        {
            return String.Format("{0}_{1}",
                apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName,
                apiDesc.ActionDescriptor.ActionName);
        }

        public static string RelativePathSansQueryString(this ApiDescription apiDesc)
        {
            return apiDesc.RelativePath.Split('?').First();
        }

        public static bool IsMarkedObsolete(this ApiDescription apiDesc)
        {
            return apiDesc.ActionDescriptor.GetCustomAttributes<ObsoleteAttribute>().Any();
        }
    }
}
