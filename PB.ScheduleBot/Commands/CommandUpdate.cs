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
        private readonly IUpdateQueryCallbackProcessor updateQueryCallbackProcessor;
        private readonly IUpdateInlineQueryProcessor updateInlineQueryProcessor;
        private readonly ILogger logger;

        public CommandUpdate(IUpdateMessageProcessor updateMessageProcessor,
                             IUpdateInlineResultProcessor updateInlineResultProcessor,
                             IUpdateQueryCallbackProcessor updateQueryCallbackProcessor,
                             IUpdateInlineQueryProcessor updateInlineQueryProcessor,
                             ILogger logger)
        {
            this.updateMessageProcessor = updateMessageProcessor;
            this.updateInlineResultProcessor = updateInlineResultProcessor;
            this.updateQueryCallbackProcessor = updateQueryCallbackProcessor;
            this.updateInlineQueryProcessor = updateInlineQueryProcessor;
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
            else if (null != updateData.callback_query)
            {
                // answer to a callback button
                await updateQueryCallbackProcessor.RunAsync(updateData.callback_query);
            }
            else if (null != updateData.inline_query)
            {
                // inline query
                await updateInlineQueryProcessor.RunAsync(updateData.inline_query);
            }
        }
    }
}