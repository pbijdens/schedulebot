using System.Threading.Tasks;
using PB.ScheduleBot.API;

namespace PB.ScheduleBot.Services
{
    public class UserStateRepository : IUserStateRepository
    {
        private readonly ILogger log;

        public UserStateRepository(ILogger log)
        {
            this.log = log;
        }

        public async Task<UserState> GetStateAsync(TelegramApiUser user)
        {
            throw new System.NotImplementedException();
        }

        public async Task<UserState> PutStateAsync(TelegramApiUser user, UserState state)
        {
            throw new System.NotImplementedException();
        }
    }
}