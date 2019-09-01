using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleWebApiBot.IntegrationTests
{
    public class WebApiBotApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
        where TEntryPoint : class
    {
        private static readonly string ApplicationContext = typeof(Program).Namespace;

        public WebApiBotApplicationFactory()
        {
            Configuration = GetConfiguration();

            ConfigureLogging(Configuration);
        }

        public IConfiguration Configuration { get; }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder(new string[0])
                .UseStartup<TEntryPoint>()
                .UseConfiguration(Configuration)
                .UseSerilog();
        }

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
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
    }
}
