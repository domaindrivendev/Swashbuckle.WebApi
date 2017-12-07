using System;
using System.Collections.Generic;

namespace Swashbuckle.Swagger
{
    public interface ISwaggerProvider
    {
        SwaggerDocument GetSwagger(string rootUrl, string apiVersion);
        SwaggerDocument GetSwagger(string rootUrl, string apiVersion, AreaDescription area, IList<AreaDescription> allAreas);
    }

    public class UnknownApiVersion : Exception
    {
        public UnknownApiVersion(string apiVersion)
            : base(String.Format("Unknown API version - {0}", apiVersion))
        {}
    }
}