using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using SimpleWebApiBot.Bots;
using SimpleWebApiBot.ScenarioTests.Helpers;
using SimpleWebApiBot.Timer;
using System;
using System.IO;

namespace SimpleWebApiBot.ScenarioTests.Setup
{
    public class TestingHost : IDisposable
    {
        private static readonly string ApplicationContext = typeof(TestingHost).Namespace;

        public TestingHost()
        {
            Configuration = GetConfiguration();

            ConfigureLogging(Configuration);

            var services = new ServiceCollection();

            ConfigureServices(services);

            RootScope = services.BuildServiceProvider().CreateScope();

            Log.Verbose("----- INSTANCE CREATED - {ClassName}", GetType().Name);
        }

        public IConfiguration Configuration { get; }

        public IServiceScope RootScope { get; }

        public IServiceScope CreateScope()
        {
            return RootScope.ServiceProvider.CreateScope();
        }

        private void ConfigureLogging(IConfiguration configuration)
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

        private void ConfigureServices(ServiceCollection services)
        {
            // General infrastructure services configuration
            services.AddSingleton<IConfiguration>(sp => Configuration);
            services.AddSingleton(new LoggerFactory().AddSerilog());
            services.AddLogging();

            // Bot configuration
            services.AddTransient<IBot, ProactiveBot>();

            services.AddScoped<TestAdapter>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<TestAdapter>>();
                var adapter = new TestAdapter();

                adapter.OnTurnError = async (context, exception) =>
                {
                    logger.LogError(exception, "----- BOT ERROR - Activity: {@Activity}", context.Activity);
                    await context.SendActivityAsync($"ERROR: {exception.Message}");
                };

                return adapter;

            });

            services.AddScoped<IAdapterIntegration, TestAdapterIntegration>();

            services.AddScoped<Timers>();
            services.AddScoped<Conversations>();

        }

        private IConfiguration GetConfiguration() =>
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{GetEnvironmentName()}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

        private string GetEnvironmentName() => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Testing";

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Log.CloseAndFlush();

                    RootScope.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TestingHost()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}