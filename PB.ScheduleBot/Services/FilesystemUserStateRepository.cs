using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PB.ScheduleBot.API;
using PB.ScheduleBot.Model;

namespace PB.ScheduleBot.Services
{
    public class FilesystemUserStateRepository : IUserStateRepository
    {
        private const string UserDataDirectoryName = "userdata";
        private readonly ILogger log;
        private readonly IBotConfiguration botConfiguration;

        public FilesystemUserStateRepository(ILogger log, IBotConfiguration botConfiguration)
        {
            this.log = log;
            this.botConfiguration = botConfiguration;
        }

        public async Task<UserState> GetStateAsync(TelegramApiUser user)
        {
            string path = CalculateStateFilenameFor(user);
            try
            {
                if (File.Exists(path))
                {
                    string jsonContent = File.ReadAllText(path);
                    return await Task.FromResult(JsonConvert.DeserializeObject<UserState>(jsonContent));
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Failed to read user data from '{path}' for user '{JsonConvert.SerializeObject(user)}'");
            }
            return default(UserState);
        }

        public async Task<UserState> PutStateAsync(TelegramApiUser user, UserState state)
        {
            if (null == state)
            {
                DeleteStateForUser(user);
            }
            else
            {
                StoreStateForUser(user, state);
            }
            return await Task.FromResult(state);
        }

        private void StoreStateForUser(TelegramApiUser user, UserState state)
        {
            string path = CalculateStateFilenameFor(user);
            try
            {
                log.LogInformation($"Save state: '{JsonConvert.SerializeObject(state)}' for '{JsonConvert.SerializeObject(user)}'");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, JsonConvert.SerializeObject(state));
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Failed to store user data at '{path}' for user '{JsonConvert.SerializeObject(user)}' and state data '{JsonConvert.SerializeObject(state)}'");
            }
        }

        private void DeleteStateForUser(TelegramApiUser user)
        {
            string path = CalculateStateFilenameFor(user);
            try { File.Delete(path); }
            catch (Exception ex)
            {
                log.LogError(ex, $"Failed to delete user data at '{path}' for user '{JsonConvert.SerializeObject(user)}'");
            }
        }

        private string CalculateStateFilenameFor(TelegramApiUser user)
        {
            return Path.Combine(botConfiguration.DataFolder, UserDataDirectoryName, $"user-{user.id}.json");
        }
    }
}