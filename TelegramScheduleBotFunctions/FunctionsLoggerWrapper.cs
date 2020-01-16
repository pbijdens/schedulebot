using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramScheduleBotFunctions
{
    internal class FunctionsLoggerWrapper : PB.ScheduleBot.Services.ILogger
    {
        private readonly ILogger log;

        public FunctionsLoggerWrapper(Microsoft.Extensions.Logging.ILogger log)
        {
            this.log = log;
        }

        public void LogError(Exception ex, string line)
        {
            log.LogError(ex, line);
        }

        public void LogError(string line)
        {
            log.LogError(line);
        }

        public void LogInformation(string line)
        {
            log.LogInformation(line);
        }
    }
}
