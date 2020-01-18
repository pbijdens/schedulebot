using PB.ScheduleBot.API;
using System.Threading.Tasks;

namespace PB.ScheduleBot.Commands.UpdateProcessors
{
    public interface IUpdateQueryCallbackProcessor
    {
        Task RunAsync(TelegramApiCallbackQuery callback);
    }
}