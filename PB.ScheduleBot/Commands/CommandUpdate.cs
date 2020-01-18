using System;
using PB.ScheduleBot.Commands.UpdateProcessors;
using PB.ScheduleBot.API;
using System.Threading.Tasks;
using PB.ScheduleBot.Services;

namespace PB.ScheduleBot.Commands
{
    public class CommandUpdate : ICommandUpdate
    {
        private readonly IUpdateMessageProcessor updateMessageProcessor;
        private readonly IUpdateInlineResultProcessor updateInlineResultProcessor;
        private readonly ILogger logger;

        public CommandUpdate(IUpdateMessageProcessor updateMessageProcessor,
                             IUpdateInlineResultProcessor updateInlineResultProcessor,
                             ILogger logger)
        {
            this.updateMessageProcessor = updateMessageProcessor;
            this.updateInlineResultProcessor = updateInlineResultProcessor;
            this.logger = logger;
        }

        public async Task RunAsync(TelegramApiUpdate updateData)
        {
            logger.LogInformation($"Received update request with ID {updateData.update_id}.");

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