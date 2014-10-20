using System.Web.Http.Description;

namespace Swashbuckle.Swagger
{
    public interface IOperationFilter
    {
        void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription);
    }
}
