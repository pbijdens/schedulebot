using System;
using PB.ScheduleBot.Commands.UpdateProcessors;
using PB.ScheduleBot.API;
using System.Threading.Tasks;

namespace PB.ScheduleBot.Commands
{
    public class CommandUpdate
    {
        private readonly UpdateMessageProcessor updateMessageProcessor;
        private readonly UpdateInlineResultProcessor updateInlineResultProcessor;

        public CommandUpdate(UpdateMessageProcessor updateMessageProcessor,
                             UpdateInlineResultProcessor updateInlineResultProcessor)
        {
            this.updateMessageProcessor = updateMessageProcessor;
            this.updateInlineResultProcessor = updateInlineResultProcessor;
        }

        public async Task Run(TelegramApiUpdate updateData)
        {
            // depending on the type delegate to the correct module
            if (null != updateData.message)
            {
                await updateMessageProcessor.Run(updateData.message);
            }
            else if (null != updateData.chosen_inline_result)
            {
                await updateInlineResultProcessor.Run(updateData.chosen_inline_result);
            }
        }
    }
}