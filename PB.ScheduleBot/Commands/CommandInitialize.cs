using System;
using System.IO;
using System.Threading.Tasks;
using PB.ScheduleBot.API;
using PB.ScheduleBot.Services;

namespace PB.ScheduleBot.Commands
{
    public class CommandInitialize : ICommandInitialize
    {
        private readonly ITelegramAPI api;
        private readonly ILogger log;

        public CommandInitialize(ITelegramAPI api, ILogger log)
        {
            this.api = api;
            this.log = log;
        }

        public async Task RunAsync(string baseUrl)
        {
            log.LogInformation("Installing webhook for telegram bot with configured token.");

            await api.SetWebHookAsync($"{baseUrl}");
        }
    }
}