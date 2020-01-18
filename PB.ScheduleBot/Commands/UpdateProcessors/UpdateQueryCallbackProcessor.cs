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

        public UpdateQueryCallbackProcessor(ILogger logger, 
            ITelegramAPI api,
            IUserSessionStateMachine userSessionStateMachine)
        {
            this.logger = logger;
            this.api = api;
            this.userSessionStateMachine = userSessionStateMachine;
        }

        public async Task RunAsync(TelegramApiCallbackQuery callback)
        {
            await userSessionStateMachine.ProcessQueryCallbackAsync(callback);
        }
    }
}
