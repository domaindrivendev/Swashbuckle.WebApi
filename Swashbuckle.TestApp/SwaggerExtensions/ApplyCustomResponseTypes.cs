using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Models;

namespace Swashbuckle.TestApp.SwaggerExtensions
{
    public class ApplyCustomResponseTypes : IOperationFilter
    {
        public void Apply(
            ApiDescription apiDescription,
            OperationSpec operationSpec,
            ModelSpecRegistrar modelSpecRegistrar,
            ModelSpecGenerator modelSpecGenerator)
        {
            var responseTypeAttr = apiDescription.ActionDescriptor.GetCustomAttributes<ResponseTypeAttribute>().FirstOrDefault();
            if (responseTypeAttr == null) return;

            IEnumerable<ModelSpec> complexSpecs;
            var modelSpec = modelSpecGenerator.TypeToModelSpec(responseTypeAttr.Type, out complexSpecs);

            if (modelSpec.Type == "object")
            {
                operationSpec.Type = modelSpec.Id;
            }
            else
            {
                operationSpec.Type = modelSpec.Type;
                operationSpec.Format = modelSpec.Format;
                operationSpec.Items = modelSpec.Items;
                operationSpec.Enum = modelSpec.Enum;
            }
            modelSpecRegistrar.RegisterMany(complexSpecs);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ResponseTypeAttribute : Attribute
    {
        public ResponseTypeAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; set; }
    }
}