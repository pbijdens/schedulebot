using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PB.ScheduleBot.API;
using PB.ScheduleBot.Commands;
using PB.ScheduleBot.Commands.UpdateProcessors;
using PB.ScheduleBot.Services;

namespace TelegramScheduleBotWebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Plumbing
            services.AddSingleton<PB.ScheduleBot.Services.ILogger, AspNetCoreLoggerWrapper>();
            services.AddSingleton<ITelegramAPI, TelegramAPI>();
            services.AddTransient<IUserStateRepository, UserStateRepository>();

            // Engines
            services.AddTransient<IUpdateInlineResultProcessor, UpdateInlineResultProcessor>();
            services.AddTransient<IUpdateMessageProcessor, UpdateMessageProcessor>();

            // Command handlers
            services.AddTransient<ICommandUpdate, CommandUpdate>();
            services.AddTransient<ICommandInitialize, CommandInitialize>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
