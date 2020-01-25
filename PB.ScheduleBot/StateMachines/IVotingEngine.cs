using PB.ScheduleBot.API;
using System.Threading.Tasks;

namespace PB.ScheduleBot.StateMachines
{
    public interface IVotingEngine
    {
        Task ProcessCallbackQueryAsync(TelegramApiCallbackQuery callback);
    }
}