using PB.ScheduleBot.API;
using System.Threading.Tasks;

namespace PB.ScheduleBot.StateMachines
{
    public interface IUserSessionStateMachine
    {
        Task CreateNewPollAsync(TelegramApiUser user);
        Task ProcessTextInputAsync(TelegramApiUser user, string message);
        Task UpdateUserSessionChatAsync(TelegramApiUser user);
        Task GotoShowListStateAsync(TelegramApiUser user);
        Task ProcessQueryCallbackAsync(TelegramApiCallbackQuery callback);
    }
}