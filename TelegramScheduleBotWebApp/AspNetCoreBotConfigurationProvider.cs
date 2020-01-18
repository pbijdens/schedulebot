using Microsoft.Extensions.Configuration;
using PB.ScheduleBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramScheduleBotWebApp
{
    public class AspNetCoreBotConfigurationProvider : IBotConfiguration
    {
        private readonly IConfiguration configuration;

        public AspNetCoreBotConfigurationProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string DataFolder => configuration["DataFolder"];

        public string Token => configuration["Token"];
    }
}
