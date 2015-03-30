using System;

namespace Swashbuckle.Dummy.SwaggerExtensions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class SwaggerResponseModelAttribute : Attribute
    {
        public string OperationId { get; private set; }

        public string StatusCode { get; private set; }

        public string Description { get; private set; }

        public Type ModelType { get; private set; }

        public SwaggerResponseModelAttribute(string operationId, string statusCode, string description, Type type)
        {
            this.OperationId = operationId;
            this.StatusCode = statusCode;
            this.Description = description;
            this.ModelType = type;
        }

        public SwaggerResponseModelAttribute(string operationId, string statusCode, string description) : this (operationId, statusCode, description, null){}

    }
}
