using System.Collections.Generic;

namespace PB.ScheduleBot.Model
{
    public class UserState
    {
        public enum States
        {
            EditActivePoll,
            ListOwnedPolls,
            EditSubject,
            SelectType,
            AddVotingOption,
            RemoveVotingOption,
            RenameVotingOption,
            AskForPollOptionNewName,
        }

        public UserState()
        {
        }

        public States State { get; set; }
        public List<string> OwnedPolls { get; set; }
        public string ActivePollID { get; set; }
        public string ActionData { get; set; }
    }
}