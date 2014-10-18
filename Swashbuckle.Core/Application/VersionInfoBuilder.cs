using System;
using System.Collections.Generic;
using System.Linq;
using Swashbuckle.Swagger20;

namespace Swashbuckle.Application
{
    public class VersionInfoBuilder
    {
        private readonly Dictionary<string, InfoBuilder> _versionInfos;

        public VersionInfoBuilder()
        {
            _versionInfos = new Dictionary<string, InfoBuilder>();
        }

        public InfoBuilder Version(string version, string title)
        {
            var infoBuilder = new InfoBuilder(version, title);
            _versionInfos[version] = infoBuilder;
            return infoBuilder;
        }

        public IDictionary<string, Info> Build()
        {
            return _versionInfos.ToDictionary(entry => entry.Key, entry => entry.Value.Build());
        }
    }
}
