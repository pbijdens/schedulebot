using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using PB.ScheduleBot.API;

namespace PB.ScheduleBot.Commands
{
    internal class CommandInitialize
    {
        private readonly TelegramAPI api;
        private readonly ILogger log;

        public CommandInitialize(TelegramAPI api, ILogger log)
        {
            this.api = api;
            this.log = log;
        }

        public void Run(string baseUrl)
        {
            log.LogInformation("Installing webhook for telegram bot with configured token.");

            api.SetWebHook($"{baseUrl}");
        }
    }
}