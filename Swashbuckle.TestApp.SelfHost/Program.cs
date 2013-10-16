using System;
using System.Web.Http.SelfHost;
using Swashbuckle.TestApp.Api.App_Start;

namespace Swashbuckle.TestApp.SelfHost
{
    internal class Program
    {
        private static void Main(string[] args)
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