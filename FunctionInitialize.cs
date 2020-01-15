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
using PB.ScheduleBot.Commands;
using PB.ScheduleBot.API;

namespace PB.ScheduleBot
{
    public static class FunctionInitialize
    {
        [FunctionName("initialize")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ITelegramAPI api,
            ILogger log,
            ExecutionContext context,
            CommandInitialize command
            )
        {
            var config = FunctionUtils.GetConfiguration(context);
            string token = config["Token"];

            await api.SetupAsync(token);
            string baseUrl = FunctionUtils.GetBaseUrl(req);

            try
            {
                await command.Run($"{baseUrl}webhook?token={token}");
                return new OkObjectResult(true);
            }
            catch (TelegramAPIException exception)
            {
                log.LogError(exception, "Initialization failed with an exception");
                return new BadRequestObjectResult("Failed.");
            }
        }
    }
}
