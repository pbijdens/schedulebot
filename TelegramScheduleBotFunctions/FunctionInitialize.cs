using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PB.ScheduleBot.API;
using PB.ScheduleBot.Commands;

namespace TelegramScheduleBotFunctions
{
    public static class FunctionInitialize
    {
        [FunctionName("initialize")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ITelegramAPI api,
            ILogger log,
            ExecutionContext context,
            CommandInitialize command)
        {
            var config = FunctionsUtils.GetConfiguration(context);
            string token = config["Token"];

            await api.SetupAsync(token);
            string baseUrl = FunctionsUtils.GetBaseUrl(req);

            try
            {
                await command.RunAsync($"{baseUrl}webhook?token={token}");
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
