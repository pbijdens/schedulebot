using PB.ScheduleBot.API;
using System.Threading.Tasks;

namespace PB.ScheduleBot.Commands.UpdateProcessors
{
    public interface IUpdateInlineQueryProcessor
    {
        Task RunAsync(TelegramApiInlineQuery inlineQuery);
    }
}