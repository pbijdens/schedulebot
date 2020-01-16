using PB.ScheduleBot.API;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PB.ScheduleBot.Commands.UpdateProcessors
{
    public interface IUpdateMessageProcessor
    {
        Task RunAsync(TelegramApiMessage message);
    }
}
