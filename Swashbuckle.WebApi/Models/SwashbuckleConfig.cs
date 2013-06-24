using System;

namespace Swashbuckle.WebApi.Models
{
    public class SwashbuckleConfig
    {
        internal static readonly SwashbuckleConfig Instance = new SwashbuckleConfig();

        public SwashbuckleConfig()
        {
            GeneratorConfig = new GeneratorConfig();
            SwaggerUiConfig = new SwaggerUiConfig();
        }

        internal GeneratorConfig GeneratorConfig { get; private set; }
        internal SwaggerUiConfig SwaggerUiConfig { get; private set; }

        public static SwashbuckleConfig Customize()
        {
            return Instance;
        }

        public SwashbuckleConfig Generator(Action<GeneratorConfig> customize)
        {
            customize(GeneratorConfig);
            return this;
        }

        public SwashbuckleConfig SwaggerUi(Action<SwaggerUiConfig> customize)
        {
            customize(SwaggerUiConfig);
            return this;
        }
    }
}