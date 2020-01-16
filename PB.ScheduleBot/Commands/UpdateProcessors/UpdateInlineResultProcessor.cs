using System.Threading.Tasks;
using PB.ScheduleBot.API;

namespace PB.ScheduleBot.Commands.UpdateProcessors
{
    public class UpdateInlineResultProcessor : IUpdateInlineResultProcessor
    {
        private ITelegramAPI api;

        public UpdateInlineResultProcessor(ITelegramAPI api)
        {
            this.api = api;
        }

        public async Task RunAsync(TelegramApiChosenInlineResult chosenInlineResult)
        {
            await Task.FromResult(0);
        }
    }
}