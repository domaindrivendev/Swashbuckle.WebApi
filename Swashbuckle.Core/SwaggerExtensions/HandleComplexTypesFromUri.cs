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
                operation.parameters.Remove(parameter);

                var model = models[parameter.name];
                foreach (var entry in model.properties)
                {
                    var param = new Parameter
                    {
                        name = entry.Key.ToLowerInvariant(),
                        @in = "query",
                        required = model.required != null && model.required.Contains(entry.Key)
                    };
                    param.PopulateFrom(entry.Value);

                    operation.parameters.Add(param);
                }
            }
        }
    }
}