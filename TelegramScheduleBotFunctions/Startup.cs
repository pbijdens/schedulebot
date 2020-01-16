using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using PB.ScheduleBot.API;
using PB.ScheduleBot.Commands;
using PB.ScheduleBot.Commands.UpdateProcessors;
using PB.ScheduleBot.Services;

using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(TelegramScheduleBotFunctions.Startup))]
namespace TelegramScheduleBotFunctions
{

    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();

            builder.Services.AddSingleton<PB.ScheduleBot.Services.ILogger, FunctionsLoggerWrapper>();
            builder.Services.AddSingleton<ITelegramAPI, TelegramAPI>();
            builder.Services.AddTransient<IUserStateRepository, UserStateRepository>();
            builder.Services.AddTransient<IUpdateInlineResultProcessor, UpdateInlineResultProcessor>();
            builder.Services.AddTransient<IUpdateMessageProcessor, UpdateMessageProcessor>();
            builder.Services.AddTransient<ICommandUpdate, CommandUpdate>();
            builder.Services.AddTransient<ICommandInitialize, CommandInitialize>();
        }
    }
}
