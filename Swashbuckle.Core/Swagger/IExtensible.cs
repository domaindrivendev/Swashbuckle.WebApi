using System.Collections.Generic;

namespace Swashbuckle.Swagger
{
    public interface IExtensible
    {
        Dictionary<string, object> vendorExtensions { get; set; }
    }
}
