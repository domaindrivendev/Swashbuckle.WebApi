namespace Swashbuckle.Swagger2
{
    public class SwaggerGeneratorSettings
    {
        public SwaggerGeneratorSettings(
            string host,
            string virtualPathRoot,
            Info info,
            bool ignoreObsoleteActions = false)
        {
            Host = host;
            VirtualPathRoot = virtualPathRoot;
            Info = info;
            IgnoreObsoleteActions = ignoreObsoleteActions;
        }

        public string Host { get; private set; }

        public string VirtualPathRoot { get; private set; }

        public Info Info { get; private set; }

        public bool IgnoreObsoleteActions { get; private set; }
    }
}