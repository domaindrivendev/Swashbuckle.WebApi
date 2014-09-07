using System;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Configuration;
using Swashbuckle.Swagger2;

namespace Swashbuckle.Application
{
    public static class HttpExtensions
    {
        private const string SwaggerConfigKey = "Swashbuckle_SwaggerConfig";

        public static void SetSwaggerConfig(this HttpConfiguration httpConfig, Swagger2Config swaggerConfig)
        {
            httpConfig.Properties[SwaggerConfigKey] = swaggerConfig;
        }

        public static Swagger2Config GetSwaggerConfig(this HttpConfiguration httpConfig)
        {
            var swaggerConfig = httpConfig.Properties[SwaggerConfigKey] as Swagger2Config;
            if (swaggerConfig == null)
                throw new InvalidOperationException("SwaggerConfig not found in HttpConfiguration properties");
            return swaggerConfig;
        }

        public static ISwaggerProvider SwaggerProvider(this HttpRequestMessage request)
        {
            var httpConfig = request.GetConfiguration();
            var swaggerConfig = httpConfig.GetSwaggerConfig();

            return swaggerConfig.GetSwaggerProvider(request);
        }
    }
}