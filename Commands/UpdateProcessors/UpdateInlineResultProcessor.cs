using System.Threading.Tasks;
using PB.ScheduleBot.API;

namespace PB.ScheduleBot.Commands.UpdateProcessors
{
    public class UpdateInlineResultProcessor
    {
        private ITelegramAPI api;

        public UpdateInlineResultProcessor(ITelegramAPI api)
        {
            this.api = api;
        }

        public async Task Run(TelegramApiChosenInlineResult chosenInlineResult)
        {
            await Task.FromResult(0);
        }
    }
}