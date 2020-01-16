using System;
using PB.ScheduleBot.Commands.UpdateProcessors;
using PB.ScheduleBot.API;
using System.Threading.Tasks;

namespace PB.ScheduleBot.Commands
{
    public class CommandUpdate : ICommandUpdate
    {
        private readonly IUpdateMessageProcessor updateMessageProcessor;
        private readonly IUpdateInlineResultProcessor updateInlineResultProcessor;

        public CommandUpdate(IUpdateMessageProcessor updateMessageProcessor,
                             IUpdateInlineResultProcessor updateInlineResultProcessor)
        {
            this.updateMessageProcessor = updateMessageProcessor;
            this.updateInlineResultProcessor = updateInlineResultProcessor;
        }

        public async Task RunAsync(TelegramApiUpdate updateData)
        {
            // depending on the type delegate to the correct module
            if (null != updateData.message)
            {
                await updateMessageProcessor.RunAsync(updateData.message);
            }
            else if (null != updateData.chosen_inline_result)
            {
                await updateInlineResultProcessor.RunAsync(updateData.chosen_inline_result);
            }
        }
    }
}