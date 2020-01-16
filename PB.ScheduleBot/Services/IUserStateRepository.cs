using System.Threading.Tasks;
using PB.ScheduleBot.API;
using PB.ScheduleBot.Services;

namespace PB.ScheduleBot.Services
{
    public interface IUserStateRepository
    {
        Task<UserState> GetStateAsync(TelegramApiUser user);
        Task<UserState> PutStateAsync(TelegramApiUser user, UserState state);
    }
}