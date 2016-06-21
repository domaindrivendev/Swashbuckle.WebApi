using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace Swashbuckle.Tests.Owin
{
    /// <summary>
    /// Configures only selected controllers with Asp.Net.
    /// Used in setting up owin in memory pipeline with routes only
    /// from a predefined list of controllers
    /// </summary>
    public class PredefinedHttpControllerSelector : DefaultHttpControllerSelector
    {
        private readonly Type[] supportedControllers;

        /// <summary>
        /// Overrides supportedControllers param and adds all controllers to owin pipeline found in assembly.
        /// Set this to true when running tests in CI, and use false when debugging/developing for faster feedback cycle.
        /// </summary>
        private readonly bool forceIncludeAllControllers;

        public PredefinedHttpControllerSelector(HttpConfiguration configuration, Type[] supportedControllers, bool forceIncludeAllControllers) : base(configuration)
        {
            this.supportedControllers = supportedControllers;
            this.forceIncludeAllControllers = forceIncludeAllControllers;
        }

        public override IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            var m = base.GetControllerMapping();

            if (forceIncludeAllControllers)
                return m;

            return m.Where(y => supportedControllers.Contains(y.Value.ControllerType))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}