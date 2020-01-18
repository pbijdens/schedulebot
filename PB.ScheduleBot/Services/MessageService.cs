using System;
using System.Collections.Generic;
using System.Text;

namespace PB.ScheduleBot.Services
{
    public class MessageService : IMessageService
    {
        public string CommandNotSupported(string command) => $"The command <b>{command}</b> is currently not supported. Use /help to get help.";
        public string InputUnexpected() => $"I don't know how to process that. {Help()}";
        public string Help() => $"Use /new to start a new poll, use /list to list your current polls or use /refresh to continue editing..";
        public string YouHaveNoPolls() => $"You have no polls. Use /new to create one.";
        public string HereAreYourThisManyPolls(int count) => $"You have {count} poll(s):";
        public string ThereIsNoActivePollToEdit() => $"There is no active poll to edit. Use /start to create one.";
        public string EditQueryPollSubject() => $"What is the subject for this poll?";
        public string EditQueryPollType() => $"Should users be restricted to a single choice or should they be allowed to select multiple options?";
        public string EditQueryOptionToRemove() => $"Which option should be removed?";
        public string EditQueryOptionToRename()=> $"Which option should be renamed?";
        public string EditQueryPollOptionNewName() => "What is the new name you would like to use for this option?";
        public string EditQueryNewPollOptionName() => "What whould we call this option?";
    }
}
