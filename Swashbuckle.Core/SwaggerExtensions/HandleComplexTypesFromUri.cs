using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Swashbuckle.SwaggerExtensions
{
    public class HandleComplexTypesFromUri : IOperationFilter
    {
        public void Apply(Operation operation, DataTypeRegistry dataTypeRegistry, ApiDescription apiDescription)
        {
            if(operation.Parameters == null) 
                return;

            var models = dataTypeRegistry.GetModels();
            var complexParameters = operation.Parameters.Where(x => x.ParamType == "query" && !string.IsNullOrWhiteSpace(x.Name) && models.ContainsKey(x.Type)).ToArray();
            foreach (var parameter in complexParameters)
            {
                var model = models[parameter.Type];

                int indexOfParam = operation.Parameters.IndexOf(parameter);
                operation.Parameters.RemoveAt(indexOfParam);
   
                var paramsToAdd = model.Properties.Select(x => new Parameter
                {
                    Name = x.Key,
                    Description = x.Value.Description,
                    Type = x.Value.Type,
                    Format = x.Value.Format,
                    Items = x.Value.Items,
                    UniqueItems = x.Value.UniqueItems,
                    Ref = x.Value.Ref,
                    ParamType = "query",
                    Enum = x.Value.Enum,
                    Required = model.Required.Contains(x.Key)
                })
                .OrderBy(group => group.ParamType);

                foreach (var newParam in paramsToAdd)
                {
                    // insert parameters back in the same spot they were removed
                    operation.Parameters.Insert(indexOfParam, newParam);
                    indexOfParam++;
                }
            }
        }
    }
}
