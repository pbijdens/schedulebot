using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PB.ScheduleBot.API;
using PB.ScheduleBot.Commands;
using PB.ScheduleBot.Services;

namespace TelegramScheduleBotWebApp.Controllers
{
    [ApiController]
    [Route("api")]
    public class TelegramAPIController : ControllerBase
    {
        private readonly ILogger<TelegramAPIController> _logger;
        private readonly ITelegramAPI _api;
        private readonly ICommandInitialize _commandInitialize;
        private readonly ICommandUpdate _commandUpdate;
        private readonly IBotConfiguration _botConfiguration;

        public TelegramAPIController(ILogger<TelegramAPIController> logger,
            ITelegramAPI api,
            ICommandInitialize commandInitialize,
            ICommandUpdate commandUpdate,
            IBotConfiguration botConfiguration)
        {
            _logger = logger;
            _api = api;
            _commandInitialize = commandInitialize;
            _commandUpdate = commandUpdate;
            _botConfiguration = botConfiguration;
        }

        [HttpGet]
        [Route("initialize")]
        public async Task GetInitialize()
        {
            string baseUrl = GetBaseUrl(Request);

            try
            {
                await _commandInitialize.RunAsync($"{baseUrl}webhook?token={_botConfiguration.Token}");
            }
            catch (TelegramAPIException exception)
            {
                _logger.LogError(exception, "GetInitialize failed");
                throw new ApiOperationFailedException();
            }
        }

        [HttpPost]
        [Route("webhook")]
        public async Task PostWebhook([FromQuery] string token, [FromBody] TelegramApiUpdate update)
        {
            // Make sure that the request is authentic
            string expectedToken = _botConfiguration.Token;
            if (expectedToken != token)
            {
                throw new UnauthorizedAccessException("No");
            }

            // Process the request
            try
            {
                await _commandUpdate.RunAsync(update);
            }
            catch (TelegramAPIException exception)
            {
                _logger.LogError(exception, "PostWebhook failed");
                throw new ApiOperationFailedException();
            }
        }

        public string GetBaseUrl(HttpRequest req)
        {
            string encodedUrl = req.GetEncodedUrl();
            int indexOfLastSlash = encodedUrl.LastIndexOf('/');
            string baseUrl = indexOfLastSlash < 0 ? encodedUrl : encodedUrl.Substring(0, indexOfLastSlash + 1);
            if (baseUrl.StartsWith("http://"))
            {
                // force the URL to use https
                baseUrl = "https" + baseUrl.Substring(4);
            }
            return baseUrl;
        }
    }
}
