using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Http.Description;

namespace Swashbuckle.Swagger2
{
    public static class ApiDescriptionExtensions
    {
        public static string OperationId(this ApiDescription apiDescription)
        {
            return String.Format("{0}.{1}",
                apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName,
                apiDescription.ActionDescriptor.ActionName);
        }

        public static IEnumerable<string> Consumes(this ApiDescription apiDescription)
        {
            return apiDescription.SupportedRequestBodyFormatters
                .SelectMany(formatter => formatter.SupportedMediaTypes.Select(mediaType => mediaType.MediaType));
        }

        public static IEnumerable<string> Produces(this ApiDescription apiDescription)
        {
            return apiDescription.SupportedResponseFormatters
                .SelectMany(formatter => formatter.SupportedMediaTypes.Select(mediaType => mediaType.MediaType));
        }

        public static string RelativePathSansQueryString(this ApiDescription apiDescription)
        {
            return apiDescription.RelativePath.Split('?').First();
        }

        public static bool IsNotObsolete(this ApiDescription apiDescription)
        {
            return !apiDescription.ActionDescriptor.GetCustomAttributes<ObsoleteAttribute>().Any();
        }
    }
}