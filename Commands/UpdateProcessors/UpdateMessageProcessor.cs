using System;
using PB.ScheduleBot.API;

namespace PB.ScheduleBot.Commands.UpdateProcessors
{
    public class UpdateMessageProcessor
    {
        private TelegramAPI api;
        private TelegramApiMessage message;

        public UpdateMessageProcessor(TelegramAPI api, TelegramApiMessage message)
        {
            this.api = api;
            this.message = message;
        }

        public async void Run()
        {
            await api.SendMessage(message.chat.id, $"@{message.from.username} Message received.");
        }
    }
}