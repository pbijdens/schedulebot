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

namespace TelegramScheduleBotWebApp.Controllers
{
    [ApiController]
    [Route("api")]
    public class TelegramAPIController : ControllerBase
    {
        private readonly ILogger<TelegramAPIController> _logger;
        private readonly IConfiguration _config;
        private readonly ITelegramAPI _api;
        private readonly ICommandInitialize _commandInitialize;
        private readonly ICommandUpdate _commandUpdate;

        public TelegramAPIController(ILogger<TelegramAPIController> logger,
            IConfiguration config,
            ITelegramAPI api,
            ICommandInitialize commandInitialize,
            ICommandUpdate commandUpdate)
        {
            _logger = logger;
            _config = config;
            _api = api;
            _commandInitialize = commandInitialize;
            _commandUpdate = commandUpdate;
        }

        [HttpGet]
        [Route("initialize")]
        public async Task GetInitialize()
        {
            string token = _config["Token"];

            await _api.SetupAsync(token);
            string baseUrl = GetBaseUrl(Request);

            try
            {
                await _commandInitialize.RunAsync($"{baseUrl}webhook?token={token}");
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
            string expectedToken = _config["Token"];
            if (expectedToken != token)
            {
                throw new UnauthorizedAccessException("No");
            }

            await _api.SetupAsync(token);

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
