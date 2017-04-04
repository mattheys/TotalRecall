using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace TotalRecall
{
    public class Program
    {
        public static string ProgramName = "Total Recall";
        public static bool PubliclyAvailable = false;

        private static volatile bool exit = false;
        public static void Main(string[] args)
        {
            if (!File.Exists("hosting.json"))
            {
                Console.WriteLine("The file \"hosting.json\" is missing\r\n" +
                                  "This file is not automatically created and is optional,\r\n" +
                                  "however you can add additional configuration options like\r\n" +
                                  "specifying the binding urls." +
                                  "\r\n" +
                                  "{\r\n" +
                                  "  \"server.urls\": \"http://0.0.0.0:8000\"\r\n" +
                                  "}");
            }

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", optional: true)
                .Build();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseConfiguration(config)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();

            using (var trContext = new Models.TRContext())
            {
                trContext.Database.EnsureCreated();
            }

            Directory.CreateDirectory("dbs");

            host.Run();
            exit = true;
        }
    }
}
