using System.Web.Http;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle
{
    internal static class HttpConfigurationExtensions
    {
        private static readonly IContractResolver _defaultContractResolver = new DefaultContractResolver();

        public static IContractResolver GetJsonContractResolver(this HttpConfiguration configuration)
        {
            var formatter = configuration.Formatters.JsonFormatter;

            return formatter == null ? _defaultContractResolver : formatter.SerializerSettings.ContractResolver;
        }
    }
}