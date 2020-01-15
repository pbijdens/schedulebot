using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PB.ScheduleBot.Commands;
using PB.ScheduleBot.API;

namespace PB.ScheduleBot
{
    public static class HttpTriggerCSharp
    {
        [FunctionName("webhook")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            string token = req.Query["token"];
            var api = new TelegramAPI(log, token);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TelegramApiUpdate updateData = JsonConvert.DeserializeObject<TelegramApiUpdate>(requestBody);

            try
            {
                log.LogInformation($"Update: {requestBody}");
                var command = new CommandUpdate(api);
                command.Run(updateData);
            }
            catch (Exception exception)
            {
                log.LogError(exception, $"Failed for {requestBody}");
                return new BadRequestObjectResult("Failed");
            }

            return new OkObjectResult(true);

        }
    }
}
