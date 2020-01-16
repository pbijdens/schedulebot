using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace PB.ScheduleBot.Services
{
    public interface ILogger
    {
        void LogError(Exception ex, string line);
        void LogError(string line);
        void LogInformation(string line);
    }
}
