using System;
using PB.ScheduleBot.Commands.UpdateProcessors;
using PB.ScheduleBot.API;

namespace PB.ScheduleBot.Commands
{
    internal class CommandUpdate
    {
        private TelegramAPI api;

        public CommandUpdate(TelegramAPI api)
        {
            this.api = api;
        }

        public void Run(TelegramApiUpdate updateData)
        {
            // depending on the type delegate to the correct module
            if (null != updateData.message)
            {
                var processor = new UpdateMessageProcessor(api, updateData.message);
                processor.Run();
            }
            else if (null != updateData.chosen_inline_result)
            {
                var processor = new UpdateInlineResultProcessor(api, updateData.chosen_inline_result);
                processor.Run();
            }
        }
    }
}