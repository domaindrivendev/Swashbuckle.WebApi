using System;
using System.Configuration;
using System.Web.Http.SelfHost;

namespace Swashbuckle.Dummy.SelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new HttpSelfHostConfiguration("http://localhost:8080");
            bool isolateAreaSwaggers;
            bool.TryParse(ConfigurationManager.AppSettings["isolateAreaSwaggers"], out isolateAreaSwaggers);

            SwaggerConfig.Register(config, isolateAreaSwaggers);
            WebApiConfig.Register(config);

            using (var server = new HttpSelfHostServer(config))
            {
                server.OpenAsync().Wait();
                Console.WriteLine("Press Enter to quit.");
                Console.ReadLine();
            }
        }
    }
}
