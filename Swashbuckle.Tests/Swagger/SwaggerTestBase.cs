using Swashbuckle.Application;
using System;

namespace Swashbuckle.Tests.Swagger
{
    public class SwaggerTestBase : HttpMessageHandlerTestBase<SwaggerDocsHandler>
    {
        protected SwaggerTestBase(string routeTemplate)
            : base(routeTemplate)
        {}

        protected void SetUpHandler(Action<SwaggerDocsConfig> configure = null)
        {
            var swaggerDocsConfig = new SwaggerDocsConfig();
            swaggerDocsConfig.SingleApiVersion("v1", "Test API");

            configure?.Invoke(swaggerDocsConfig);

            Handler = new SwaggerDocsHandler(swaggerDocsConfig);
        }
    }
}
