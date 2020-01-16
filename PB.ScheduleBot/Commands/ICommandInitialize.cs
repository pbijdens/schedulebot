using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PB.ScheduleBot.Commands
{
    public interface ICommandInitialize
    {
        Task RunAsync(string baseUrl);
    }
}
