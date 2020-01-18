using System;
using System.Collections.Generic;
using System.Text;

namespace PB.ScheduleBot.Services
{
    public interface IBotConfiguration
    {
        string DataFolder { get; }
        string Token { get; }
    }
}
