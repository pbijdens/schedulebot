using System;
using PB.ScheduleBot.API;
using System.Linq;
using System.Threading.Tasks;
using PB.ScheduleBot.Services;

namespace PB.ScheduleBot.Commands.UpdateProcessors
{
    public class UpdateMessageProcessor : IUpdateMessageProcessor
    {
        private ITelegramAPI api;
        private readonly IUserStateRepository userStateRepository;
        private readonly IMessageService messageService;

        public UpdateMessageProcessor(ITelegramAPI api, 
            IUserStateRepository userStateRepository, 
            IMessageService messageService)
        {
            this.api = api;
            this.userStateRepository = userStateRepository;
            this.messageService = messageService;
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
            switch (commandText)
            {
                case "/start":
                    await DoStart(message);
                    break;
                default:
                    await api.SendMessageAsync(message.chat.id, messageService.CommandNotSupported(commandText));
                    break;
            }
        }

        private async Task ProcessTextInput(TelegramApiMessage message)
        {
            await api.SendMessageAsync(message.chat.id, $"Thanks for saying {message.text}");
        }

        private async Task DoStart(TelegramApiMessage message)
        {
            UserState state = new UserState();
            await userStateRepository.PutStateAsync(message.from, state);
            await api.SendMessageAsync(message.chat.id, $"That command is currently not supported.");
        }
    }
}