using System.Collections.Generic;

namespace Swashbuckle.Swagger
{
    public interface ISwaggerObject
    {
        Dictionary<string, object> vendorExtensions { get; set; }
    }
}
