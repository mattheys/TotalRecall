using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.Threading;

namespace TotalRecall
{
    public class Program
    {
        public static string ProgramName = "Total Recall";
        public static bool PubliclyAvailable = true;

        private static volatile bool exit = false;
        public static void Main(string[] args)
        {
            //Console.BufferWidth = Math.Max(Console.BufferWidth, 300);

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();

            Console.Write("Setting up database - ");
            using (var context = new Models.TRModelContext())
            {
                context.Database.EnsureCreated();
            }
            Console.WriteLine("complete");

            if (PubliclyAvailable)
            {
                new Thread(() =>
                {
                    using (var context = new Models.TRModelContext())
                    {
                        while (!exit)
                        {
                            DateTime dt = DateTime.Today.AddHours(2);
                            if (dt < DateTime.Now) dt = dt.AddDays(1);
                            while (DateTime.Now > dt || !exit)
                            {
                                Thread.Sleep(1000);
                            }
                            if (exit) break;

                            var r = from a in context.Applications
                                    join d in context.DataItems on a.ApplicationId equals d.ApplicationId
                                    where d.InsertDate > DateTime.Now.AddDays(-30)
                                    select a;

                            context.Applications.RemoveRange(context.Applications.Except(r));
                            context.SaveChanges();
                        }
                    }
                }).Start();
                new Thread(() =>
                {
                    using (var context = new Models.TRModelContext())
                    {
                        while (!exit)
                        {
                            //TODO: Something to clear up more than 100,000 entries per app
                            Thread.Sleep(1000);
                        }
                    }

                }).Start();
            }
            host.Run();
            exit = true;
        }

        public delegate void DBMaintDelegate();
        public void DBMaint() { }

    }

    public class DatabaseMaintenence
    {

    }
}
