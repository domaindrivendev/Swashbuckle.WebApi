using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Web.Http.Controllers;

namespace SwashBuckle.WebApi
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class JsonResponseConfigAttribute :  Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            // yes, this instance is from the global formatters
            JsonMediaTypeFormatter globaljsonFormatterInstance = controllerSettings.Formatters.JsonFormatter;

            controllerSettings.Formatters.Clear();

            // NOTE: do not make any changes to this formatter instance as it reference to the instance from the global formatters.
            // if you need custom settings for a particular controller(s), then create a new instance of Xml formatter and change its settings.
            controllerSettings.Formatters.Add(globaljsonFormatterInstance);
        }
    }
}
