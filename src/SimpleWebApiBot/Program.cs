using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace SimpleWebApiBot
{
    public class Program
    {
        private static readonly string ApplicationContext = typeof(Program).Namespace;

        public static int Main(string[] args)
        {
            var configuration = GetConfiguration();

            ConfigureLogging(configuration);

            try
            {
                Log.Information("----- STARTING WEB HOST ({ApplicationContext}) @{Date}", ApplicationContext, DateTime.Now);
                BuildWebHost(args, configuration)
                    .Run();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "----- Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IWebHost BuildWebHost(string[] args, IConfiguration configuration) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseConfiguration(configuration)
                .UseSerilog()
                .Build();

        private static void ConfigureLogging(IConfiguration configuration)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.WithProperty("ApplicationContext", ApplicationContext)
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .WriteTo.Console()
                .WriteTo.File(
                    $@"D:\home\LogFiles\{ApplicationContext}-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 15,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1));

            loggerConfiguration = loggerConfiguration
                .MinimumLevel.Verbose()
                .WriteTo.Seq("http://localhost:5341");

            Log.Logger = loggerConfiguration
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        private static IConfiguration GetConfiguration() =>
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{GetEnvironmentName()}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

        private static string GetEnvironmentName() => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
    }
}
