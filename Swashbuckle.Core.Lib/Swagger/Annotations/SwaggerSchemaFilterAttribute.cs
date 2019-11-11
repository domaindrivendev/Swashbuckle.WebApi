using System;

namespace Swashbuckle.Swagger.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SwaggerSchemaFilterAttribute : Attribute
    {
        public SwaggerSchemaFilterAttribute(Type filterType)
        {
            FilterType = filterType;
        }

        public Type FilterType { get; private set; }
    }
}