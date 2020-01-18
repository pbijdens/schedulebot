using System;
using System.Collections.Generic;
using System.Text;

namespace PB.ScheduleBot.Services
{
    public class MessageService : IMessageService
    {
        public string CommandNotSupported(string command) => $"The command <b>{command}</b> is currently not supported. Use /help to get help.";
    }
}
