using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Escon.SisctNET.Web
{
    public class Program
    {
        private static IConfiguration configuration;

        public static void Main(string[] args)
        {
            //CreateWebHostBuilder(args).Build().Run();
            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json");

            configuration = builder.Build();

            var host = new WebHostBuilder()
            .UseConfiguration(configuration)
            .UseWebRoot(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
            .UseUrls("http://localhost:6090/;https://localhost:6080/")
            //.UseUrls(new string[] { "http://*", "https://*" })
            .ConfigureLogging(b =>
            {
                b.SetMinimumLevel(LogLevel.Information);
                b.AddConfiguration(configuration);
                b.AddConsole();
            })
            .UseKestrel()
            .UseStartup<Startup>()
            .Build();

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
