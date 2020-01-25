using Newtonsoft.Json;
using PB.ScheduleBot.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PB.ScheduleBot.Services
{
    public class FilesystemPollRepository : IPollRepository
    {
        private const string PollDirectoryName = "polls";
        private readonly ILogger log;
        private readonly IBotConfiguration botConfiguration;

        public FilesystemPollRepository(ILogger log, IBotConfiguration botConfiguration)
        {
            this.log = log;
            this.botConfiguration = botConfiguration;
        }

        public async Task<Poll> LoadAsync(string id)
        {
            string path = CalculatePath(id);
            try
            {
                if (File.Exists(path))
                {
                    string jsonContent = File.ReadAllText(path);
                    return await Task.FromResult(JsonConvert.DeserializeObject<Poll>(jsonContent));
                }
                log.LogError($"Could not find local file for poll with ID {id}.");
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Failed to poll data from '{path}'");
            }
            return null;
        }

        public async Task<Poll> SaveAsync(Poll poll)
        {
            if (null == poll) poll = new Poll();
            poll.ModificationDate = DateTimeOffset.UtcNow;

            log.LogInformation($"Save poll: '{JsonConvert.SerializeObject(poll)}'");
            string path = CalculatePath(poll.ID);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, JsonConvert.SerializeObject(poll));
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"Failed to store poll data at '{path}' for for poll '{JsonConvert.SerializeObject(poll)}'");
            }
            return await Task.FromResult<Poll>(poll);
        }

        public string CalculatePath(string id)
        {
            return Path.Combine(botConfiguration.DataFolder, PollDirectoryName, $"poll-{id}.json");

        }
    }
}
