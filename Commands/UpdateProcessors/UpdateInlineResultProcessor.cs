using PB.ScheduleBot.API;

namespace PB.ScheduleBot.Commands.UpdateProcessors
{
    public class UpdateInlineResultProcessor
    {
        private TelegramAPI api;
        private TelegramApiChosenInlineResult chosenInlineResult;

        public UpdateInlineResultProcessor(TelegramAPI api, TelegramApiChosenInlineResult chosenInlineResult)
        {
            this.api = api;
            this.chosenInlineResult = chosenInlineResult;
        }

        public void Run()
        {

        }
    }
}