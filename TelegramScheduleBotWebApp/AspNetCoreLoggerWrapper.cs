using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramScheduleBotWebApp
{
    public class AspNetCoreLoggerWrapper : PB.ScheduleBot.Services.ILogger
    {
        private readonly ILogger _logger;

        public AspNetCoreLoggerWrapper(Microsoft.Extensions.Logging.ILogger<AspNetCoreLoggerWrapper> logger)
        {
            _logger = logger;
        }

        public void LogError(Exception ex, string line)
        {
            _logger.LogError(ex, line);
        }

        public void LogError(string line)
        {
            _logger.LogError(line);
        }

        public void LogInformation(string line)
        {
            _logger.LogInformation(line);
        }
    }
}
