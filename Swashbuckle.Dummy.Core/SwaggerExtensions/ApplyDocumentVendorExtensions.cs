using System.Web.Http.Description;
using Swashbuckle.Swagger20;

namespace Swashbuckle.Dummy.SwaggerExtensions
{
    public class ApplyDocumentVendorExtensions : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            swaggerDoc.extensions.Add("x-document", "foo");
        }
    }
}
