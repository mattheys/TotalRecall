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

        public const int DBPrunePeriodInDays = 30;  //Days that DBs should not have been written to before being deleted.
        public const int DBWarningPeriodInDays = 15;  //Days that DBs should not have been written to before sending user an email if we have one.



        public static void Main(string[] args)
        {
            if (!File.Exists("hosting.json"))
            {
                Console.WriteLine("The file \"hosting.json\" is missing\r\n" +
                                  "This file is not automatically created and is optional, however you can add additional configuration options like specifying the binding urls.\r\n" +
                                  "\r\n" +
                                  "{\r\n" +
                                  "  \"server.urls\": \"http://0.0.0.0:8000\"\r\n" +
                                  "}\r\n");
            }

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", optional: true, reloadOnChange: true)
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

            var updateCheckInterval = 0;
            var updateCheckInitialDelay = (int)(new TimeSpan(12, 0, 0)).TotalMilliseconds;

            var maintenanceInitialDelay = (int)(new TimeSpan(0, 15, 0)).TotalMilliseconds;
            var maintenanceInterval = (int)(new TimeSpan(1, 0, 0)).TotalMilliseconds;

#if DEBUG
            updateCheckInterval = Timeout.Infinite;
            updateCheckInitialDelay = Timeout.Infinite;

            maintenanceInitialDelay = 0;
            maintenanceInterval = (int)(new TimeSpan(0, 5, 0)).TotalMilliseconds;
#endif


            Timer updateTimer = new Timer(new TimerCallback(UpdateCallback), null, updateCheckInitialDelay, updateCheckInterval);
            Timer maintenanceTimer = new Timer(new TimerCallback(MaintenanceCallback), null, maintenanceInitialDelay, maintenanceInterval);

            host.Run();
        }

        static void UpdateCallback(object state)
        {
            //Need to try and work out a continuous delivery method from github.
        }
        static void MaintenanceCallback(object state)
        {
            PruneDbsFromFilesystem();
        }

        static void PruneDbsFromFilesystem()
        {
            var Context = new Models.TRContext();

            foreach (var item in new DirectoryInfo("dbs").GetFiles("*.db"))
            {
                try
                {
                    if ((DateTime.Now - item.LastAccessTime).TotalDays > DBPrunePeriodInDays)
                    {
                        if (Guid.TryParse(Path.GetFileNameWithoutExtension(item.FullName), out Guid publicKey))
                        {
                            var app = Context.Applications.Where(q => q.PublicKey == publicKey).FirstOrDefault();
                            Context.Applications.Remove(app);
                            Context.SaveChanges();
                        }
                        item.Delete();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

    }
}
