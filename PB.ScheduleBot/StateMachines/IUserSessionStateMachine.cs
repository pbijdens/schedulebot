using PB.ScheduleBot.API;
using System.Threading.Tasks;

namespace PB.ScheduleBot.StateMachines
{
    public interface IUserSessionStateMachine
    {
        Task CreateNewPollAsync(TelegramApiUser user);
        Task ProcessTextInputAsync(TelegramApiUser user, long messageID, string message);
        Task UpdateUserChatSessionForStateAsync(TelegramApiUser user);
        Task GotoShowListStateAsync(TelegramApiUser user);
        Task ProcessCallbackQueryAsync(TelegramApiCallbackQuery callback);
        Task ResetPrivateMessageHistory(TelegramApiUser user);
    }
}