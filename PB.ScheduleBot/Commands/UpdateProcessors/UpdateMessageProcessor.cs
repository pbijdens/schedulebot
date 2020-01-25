using System;
using PB.ScheduleBot.API;
using System.Linq;
using System.Threading.Tasks;
using PB.ScheduleBot.Services;
using PB.ScheduleBot.Model;
using System.Collections.Generic;
using PB.ScheduleBot.StateMachines;

namespace PB.ScheduleBot.Commands.UpdateProcessors
{
    public class UpdateMessageProcessor : IUpdateMessageProcessor
    {
        private ITelegramAPI api;
        private readonly IUserSessionStateMachine sessionStateMachine;
        private readonly IMessageService messageService;
        private readonly ILogger logger;

        public UpdateMessageProcessor(ITelegramAPI api, 
            IUserSessionStateMachine sessionStateMachine,
            IMessageService messageService,
            ILogger logger)
        {
            this.api = api;
            this.sessionStateMachine = sessionStateMachine;
            this.messageService = messageService;
            this.logger = logger;
        }

        public async Task RunAsync(TelegramApiMessage message)
        {
            if (message.chat.type != "private") return; // Only supporting private chats

            TelegramApiMessageEntity command = message.entities?.Where(x => x.type == "bot_command").FirstOrDefault();
            if (default(TelegramApiMessageEntity) != command)
            {
                await ProcessCommand(message, command);
            }
            else
            {
                await ProcessTextInput(message);
            }
        }

        private async Task ProcessCommand(TelegramApiMessage message, TelegramApiMessageEntity command)
        {
            string commandText = message.text.Substring(command.offset, command.length).ToLowerInvariant().Split('@')[0];
            logger.LogInformation($"Processing command {commandText}");
            switch (commandText)
            {
                case "/new":
                    await sessionStateMachine.CreateNewPollAsync(message.from);
                    break;
                case "/start":
                case "/help":
                    await ShowHelp(message.from);
                    break;
                case "/list":
                    await sessionStateMachine.GotoShowListStateAsync(message.from);
                    break;
                case "/refresh":
                    await sessionStateMachine.UpdateUserChatSessionForStateAsync(message.from);
                    break;
                default:
                    await api.SendMessageAsync(message.chat.id, messageService.CommandNotSupported(commandText));
                    break;
            }
            // on any command, forget that we were editing messages
            await sessionStateMachine.ResetPrivateMessageHistory(message.from);
        }

        private async Task ProcessTextInput(TelegramApiMessage message)
        {
            await sessionStateMachine.ProcessTextInputAsync(message.from, message.message_id, message.text);
        }

        private async Task ShowHelp(TelegramApiUser from)
        {
            await api.SendMessageAsync(from.id, messageService.Help());
        }
    }
}