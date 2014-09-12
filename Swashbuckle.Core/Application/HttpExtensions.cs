using System;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Configuration;
using Swashbuckle.Swagger20;

namespace Swashbuckle.Application
{
    public static class HttpExtensions
    {
        private const string SwaggerConfigKey = "Swashbuckle_SwaggerConfig";

        public static void SetSwaggerConfig(this HttpConfiguration httpConfig, Swagger20Config swaggerConfig)
        {
            httpConfig.Properties[SwaggerConfigKey] = swaggerConfig;
        }

        public static Swagger20Config GetSwaggerConfig(this HttpConfiguration httpConfig)
        {
            var swaggerConfig = httpConfig.Properties[SwaggerConfigKey] as Swagger20Config;
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