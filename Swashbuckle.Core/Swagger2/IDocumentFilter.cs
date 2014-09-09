using System.Web.Http.Description;

namespace Swashbuckle.Swagger2
{
    public interface IDocumentFilter
    {
        void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer);
    }
}
