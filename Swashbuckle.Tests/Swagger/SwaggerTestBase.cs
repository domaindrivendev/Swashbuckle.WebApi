using System;
using System.Web.Http;
using Newtonsoft.Json;
using Swashbuckle.Application;
using Swashbuckle.Swagger;
using NUnit.Framework;

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
            swaggerDocsConfig.SingleApiVersion("1.0", "Test API");

            if (configure != null)
                configure(swaggerDocsConfig);

            Handler = new SwaggerDocsHandler(swaggerDocsConfig);
        }
    }
}
