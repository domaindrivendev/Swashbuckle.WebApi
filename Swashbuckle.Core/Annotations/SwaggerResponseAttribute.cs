namespace Swashbuckle.Annotations
{
    using System;
    using System.Net;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class SwaggerResponseAttribute : Attribute
    {
        public SwaggerResponseAttribute(HttpStatusCode statusCode, Type type, string description = null)
        {
            this.StatusCode = (int)statusCode;
            this.Description = description;
            this.ResponseType = type;
        }

        public SwaggerResponseAttribute(int statusCode, Type type, string description = null)
        {
            this.StatusCode = statusCode;
            this.Description = description;
            this.ResponseType = type;
        }

        public SwaggerResponseAttribute(HttpStatusCode statusCode, string description = null)
        {
            this.StatusCode = (int)statusCode;
            this.Description = description;
        }

        public SwaggerResponseAttribute(int statusCode, string description = null)
        {
            this.StatusCode = statusCode;
            this.Description = description;
        }

        public int StatusCode { get; private set; }

        public string Description { get; private set; }

        public Type ResponseType { get; private set; }
    }
}