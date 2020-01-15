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
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context
            )
        {
            var config = FunctionUtils.GetConfiguration(context);
            string token = config["Token"];

            var api = new TelegramAPI(log, token);
            string baseUrl = FunctionUtils.GetBaseUrl(req);

            try
            {
                var command = new CommandInitialize(api, log);
                command.Run($"{baseUrl}webhook?token={token}");

                return new OkObjectResult(true);
            }
            catch (TelegramAPIException exception)
            {
                log.LogError(exception, "Initialization failed with an exception");
                return new BadRequestObjectResult("Initialization failed. See log for details.");
            }
        }
    }
}
