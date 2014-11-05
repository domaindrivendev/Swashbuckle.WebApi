using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Swashbuckle.SwaggerExtensions
{
    public class HandleComplexTypesFromUri : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if(operation.parameters == null)
                return;
            
            var models = schemaRegistry.Definitions;
            var complexParameters = operation.parameters.Where(x => x.@in == "query" && !string.IsNullOrWhiteSpace(x.name) && models.ContainsKey(x.name)).ToArray();
            foreach (var parameter in complexParameters)
            {
                var model = models[parameter.name];

                operation.parameters.Remove(parameter);

                model.properties.Select(x => new Parameter
                {
                    name = x.Key.ToLowerInvariant(),
                    type = x.Value.type,
                    @in = "query",
                    required = model.required.Contains(x.Key)
                }).AddTo(operation.parameters);
            }
        }
    }
}