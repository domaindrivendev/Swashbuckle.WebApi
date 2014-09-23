using System;
using System.Collections.Generic;
using System.Linq;
using Swashbuckle.Swagger20;

namespace Swashbuckle.Configuration
{
    public class ApiVersionsBuilder
    {
        private readonly Dictionary<string, InfoBuilder> _apiVersions;

        public ApiVersionsBuilder()
        {
            _apiVersions = new Dictionary<string, InfoBuilder>();
        }

        public InfoBuilder Version(string version, string title)
        {
            var infoBuilder = new InfoBuilder(version, title);
            _apiVersions[version] = infoBuilder;
            return infoBuilder;
        }

        public IDictionary<string, Info> Build()
        {
            return _apiVersions.ToDictionary(entry => entry.Key, entry => entry.Value.Build());
        }
    }
}
