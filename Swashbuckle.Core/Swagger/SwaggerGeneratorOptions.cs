using System;
using System.Collections.Generic;
using System.Web.Http.Description;

namespace Swashbuckle.Swagger
{
    public class SwaggerGeneratorOptions
    {
        public SwaggerGeneratorOptions(
            Func<ApiDescription, string> resourceNameResolver,
            IDictionary<Type, Func<DataType>> customTypeMappings,
            IEnumerable<PolymorphicType> polymorphicTypes,
            IEnumerable<IModelFilter> modelFilters,
            IEnumerable<IOperationFilter> operationFilters)
        {
            ResourceNameResolver = resourceNameResolver;
            CustomTypeMappings = customTypeMappings;
            PolymorphicTypes = polymorphicTypes;
            ModelFilters = modelFilters;
            OperationFilters = operationFilters;
        }

        public Func<ApiDescription, string> ResourceNameResolver { get; private set; }

        public IDictionary<Type, Func<DataType>> CustomTypeMappings { get; private set; }

        public IEnumerable<PolymorphicType> PolymorphicTypes { get; private set; }

        public IEnumerable<IModelFilter> ModelFilters { get; private set; }

        public IEnumerable<IOperationFilter> OperationFilters { get; private set; }
    }
}