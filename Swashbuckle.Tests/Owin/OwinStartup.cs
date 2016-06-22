using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using Owin;
using Swashbuckle.Application;

namespace Swashbuckle.Tests.Owin
{
    public class OwinStartup
    {
        private Type[] supportedControllers = { };
        //private Func<IApiExplorer, Func<IOperationFilter>> operationIdFunc;

        /// <summary>
        /// When developing, it is quicker to run tests scoped to just a list of controllers.
        /// When running tests in CI, set this to true.
        /// 
        /// Setting this to true ensures that swagger config generation is tested to work in tandem with all controllers,
        /// not just scoped to single controllers.
        /// </summary>
        public static bool ForceIncludeAllControllers = false;

        public OwinStartup(params Type[] supportedControllers)
        {
            this.supportedControllers = supportedControllers;
        }

        public void Configuration(IAppBuilder app)
        {
            if (supportedControllers == null)
                supportedControllers = new Type[] { };

            var config = new HttpConfiguration();

            config.Services.Replace(typeof(IHttpControllerSelector), new PredefinedHttpControllerSelector(config, supportedControllers, ForceIncludeAllControllers));

            //Route Constraints
            var constraintResolver = new DefaultInlineConstraintResolver();

            config.MapHttpAttributeRoutes(constraintResolver);

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = System.Web.Http.RouteParameter.Optional } // optional id
                );

            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            ConfigureFormatters(config);

            config.EnsureInitialized();

            EnableSwagger(config);

            app.UseWebApi(config);
        }

        protected virtual void EnableSwagger(HttpConfiguration config)
        {
            config
                .EnableSwagger(c => c.SingleApiVersion("v1", "A title for your API"))
                .EnableSwaggerUi();
        }

        protected virtual void ConfigureFormatters(HttpConfiguration config)
        {
            // remove application/x-www-form-urlencoded formatters
            var mediaTypeFormatters = config.Formatters.Where(y => y.SupportedMediaTypes.Any(c => c.MediaType == "application/x-www-form-urlencoded")).ToList();
            mediaTypeFormatters.ForEach(x => config.Formatters.Remove(x));

            //Xml Formatter
            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}