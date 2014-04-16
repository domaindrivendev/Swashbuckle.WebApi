using System;
using System.Web.Http.SelfHost;

namespace Swashbuckle.TestApp.SelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new HttpSelfHostConfiguration("http://localhost:8080");

            WebApiConfig.Register(config);
            SwaggerConfig.Register(config);

            using (var server = new HttpSelfHostServer(config))
            {
                server.OpenAsync().Wait();
                Console.WriteLine("Press Enter to quit.");
                Console.ReadLine();
            }
        }
    }
}
