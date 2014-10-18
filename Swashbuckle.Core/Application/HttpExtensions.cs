using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger20;
using Swashbuckle.WebAssets;
using System;

namespace Swashbuckle.Application
{
    public static class HttpExtensions
    {
        private const string SwaggerDocsConfigKey = "Swashbuckle_SwaggerConfig";
        private const string SwaggerUiConfigKey = "Swashbuckle_SwaggerUiConfig";

        public static void SetSwaggerDocsConfig(this HttpConfiguration httpConfig, SwaggerDocsConfig swaggerDocsConfig)
        {
            httpConfig.Properties[SwaggerDocsConfigKey] = swaggerDocsConfig;
        }

        public static SwaggerDocsConfig GetSwaggerDocsConfig(this HttpConfiguration httpConfig)
        {
            var swaggerDocsConfig = httpConfig.Properties[SwaggerDocsConfigKey] as SwaggerDocsConfig;
            if (swaggerDocsConfig == null)
                throw new InvalidOperationException("SwaggerDocsConfig not found in HttpConfiguration properties");
            return swaggerDocsConfig;
        }

        public static void SetSwaggerUiConfig(this HttpConfiguration httpConfig, SwaggerUi20Config swaggerUiConfig)
        {
            httpConfig.Properties[SwaggerUiConfigKey] = swaggerUiConfig;
        }

        public static SwaggerUi20Config GetSwaggerUiConfig(this HttpConfiguration httpConfig)
        {
            var swaggerUiConfig = httpConfig.Properties[SwaggerUiConfigKey] as SwaggerUi20Config;
            if (swaggerUiConfig == null)
                throw new InvalidOperationException("SwaggerUiConfig not found in HttpConfiguration properties");
            return swaggerUiConfig;
        }

        public static ISwaggerProvider SwaggerProvider(this HttpRequestMessage request)
        {
            var httpConfig = request.GetConfiguration();
            var swaggerDocsConfig = httpConfig.GetSwaggerDocsConfig();
            return swaggerDocsConfig.GetSwaggerProvider(request);
        }

        public static IWebAssetProvider SwaggerUiProvider(this HttpRequestMessage request)
        {
            var httpConfig = request.GetConfiguration();
            var swaggerUiConfig = httpConfig.GetSwaggerUiConfig();
            return swaggerUiConfig.GetSwaggerUiProvider(request);
        }
    }
}