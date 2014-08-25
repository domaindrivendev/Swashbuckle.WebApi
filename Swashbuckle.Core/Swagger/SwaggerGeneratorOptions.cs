using System;
using System.Collections.Generic;
using System.Web.Http.Description;

namespace Swashbuckle.Swagger
{
    public class SwaggerGeneratorOptions
    {
        public SwaggerGeneratorOptions(
            Func<ApiDescription, string> resourceNameResolver,
            IComparer<string> resourceNameComparer,
            IDictionary<Type, Func<DataType>> customTypeMappings,
            IEnumerable<PolymorphicType> polymorphicTypes,
            IEnumerable<IModelFilter> modelFilters,
            IEnumerable<IOperationFilter> operationFilters,
            Info apiInfo,
            IDictionary<string, Authorization> authorizations)
        {
            ResourceNameResolver = resourceNameResolver;
            ResourceNameComparer = resourceNameComparer;
            CustomTypeMappings = customTypeMappings;
            PolymorphicTypes = polymorphicTypes;
            ModelFilters = modelFilters;
            OperationFilters = operationFilters;
            ApiInfo = apiInfo;
            Authorizations = authorizations;
        }

        public Func<ApiDescription, string> ResourceNameResolver { get; private set; }

        public IComparer<string> ResourceNameComparer { get; private set; }

        public IDictionary<Type, Func<DataType>> CustomTypeMappings { get; private set; }

        public IEnumerable<PolymorphicType> PolymorphicTypes { get; private set; }

        public IEnumerable<IModelFilter> ModelFilters { get; private set; }

        public IEnumerable<IOperationFilter> OperationFilters { get; private set; }

        public Info ApiInfo { get; private set; }

        public IDictionary<string, Authorization> Authorizations { get; private set; }
    }
}