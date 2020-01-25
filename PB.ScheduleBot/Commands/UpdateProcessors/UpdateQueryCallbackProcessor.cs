using PB.ScheduleBot.API;
using PB.ScheduleBot.Services;
using PB.ScheduleBot.StateMachines;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PB.ScheduleBot.Commands.UpdateProcessors
{
    public class UpdateQueryCallbackProcessor : IUpdateQueryCallbackProcessor
    {
        private readonly ILogger logger;
        private readonly ITelegramAPI api;
        private readonly IUserSessionStateMachine userSessionStateMachine;
        private readonly IVotingEngine votingEngine;

        public UpdateQueryCallbackProcessor(ILogger logger, 
            ITelegramAPI api,
            IUserSessionStateMachine userSessionStateMachine,
            IVotingEngine votingEngine)
        {
            this.logger = logger;
            this.api = api;
            this.userSessionStateMachine = userSessionStateMachine;
            this.votingEngine = votingEngine;
        }

        public async Task RunAsync(TelegramApiCallbackQuery callback)
        {
            if (!string.IsNullOrEmpty(callback.inline_message_id))
            {
                await votingEngine.ProcessCallbackQueryAsync(callback);
                await api.AnswerCallbackQuery(callback.id);
            }
            else
            {
                await userSessionStateMachine.ProcessCallbackQueryAsync(callback);
            }
        }
    }
}
