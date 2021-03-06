﻿using Datalust.SerilogMiddlewareExample.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleWebApiBot.Bots;
using SimpleWebApiBot.Controllers;
using SimpleWebApiBot.Timer;
using System;

namespace SimpleWebApiBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<SerilogMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            app.UseBotFramework();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSingleton<IAdapterIntegration>(sp => 
            {
                var logger = sp.GetRequiredService<ILogger<IAdapterIntegration>>();
                var appId = Configuration["BotWebApiApp:AppId"];
                var appPassword = Configuration["BotWebApiApp:AppPassword"];

                var adapter = new BotFrameworkAdapter(
                    credentialProvider: new SimpleCredentialProvider(appId, appPassword),
                    logger: logger);

                adapter.OnTurnError = async (context, exception) =>
                {
                    logger.LogError(exception, "----- SimpleWebApiBot ERROR");
                    await context.SendActivityAsync("Sorry, it looks like something went wrong.");
                };

                return adapter;
            });

            services.AddSingleton<Timers>();
            services.AddSingleton<Conversations>();

            services.AddTransient<IBot, ProactiveBot>();
        }
    }
}