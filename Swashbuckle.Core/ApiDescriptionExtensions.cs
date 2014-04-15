using System;
using System.Linq;
using System.Web.Http.Description;

namespace Swashbuckle
{
    internal static class ApiDescriptionExtensions
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

        public static Type ActualResponseType(this ApiDescription apiDesc)
        {
            // HACK: The ResponseDescription property was introduced in WebApi 5.0 but Swashbuckle requires >= 4.0. The reflection hack below provides support for
            // the ResponseType attribute if the application is running against a version of WebApi that supports it.
            var apiDescType = typeof (ApiDescription);

            var responseDescPropInfo = apiDescType.GetProperty("ResponseDescription");
            if (responseDescPropInfo != null)
            {
                var responseDesc = responseDescPropInfo.GetValue(apiDesc, null);
                if (responseDesc != null)
                {
                    var responseDescType = responseDesc.GetType();

                    var responseTypePropInfo = responseDescType.GetProperty("ResponseType");
                    if (responseTypePropInfo != null)
                    {
                        var responseType = responseTypePropInfo.GetValue(responseDesc, null);
                        if (responseType != null)
                            return (Type) responseType;
                    }
                }
            }

            // Otherwise, it defaults to the declared response type
            return apiDesc.ActionDescriptor.ReturnType;
        }
    }
}
