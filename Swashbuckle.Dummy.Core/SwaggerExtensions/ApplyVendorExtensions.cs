using System.Web.Http.Description;
using Swashbuckle.Swagger2;

namespace Swashbuckle.Dummy.SwaggerExtensions
{
    public class ApplyVendorExtensions : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            swaggerDoc.extensions.Add("x-foo", "bar");
        }
    }
}
