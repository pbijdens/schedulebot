using System;
using System.Collections.Generic;
using System.Text;

namespace PB.ScheduleBot.Services
{
    public class MessageServiceEnglish : IMessageService
    {
        public string CommandNotSupported(string command) => $"The command <b>{command}</b> is currently not supported. Use /help to get help.";
        public string InputUnexpected() => $"I don't know how to process that. {Help()}";
        public string Help() => $"Use /new to create a new poll, use /list to list your current polls or use /refresh to continue editing..";
        public string YouHaveNoPolls() => $"You have no polls. Use /new to create one.";
        public string HereAreYourThisManyPolls(int count) => $"You have {count} poll(s):";
        public string ThereIsNoActivePollToEdit() => $"There is no active poll to edit. Use /start to create one.";
        public string EditQueryPollSubject() => $"What is the subject for this poll?";
        public string EditQueryPollType() => $"Should users be restricted to a single choice or should they be allowed to select multiple options?";
        public string EditQueryOptionToRemove() => $"Which option should be removed?";
        public string EditQueryOptionToRename() => $"Which option should be renamed?";
        public string EditQueryPollOptionNewName() => "What is the new name you would like to use for this option?";
        public string EditQueryNewPollOptionName() => "What should this new option be called?";
        public string PollMessageNoSubjectText() => "No subject was set for this this (yet)";
        public string PollMessagePollIsClosed() => "This poll is closed, you can no longer vote";
        public string PollMessagePollHasNoVotingOptions() => "Vote options have not been set up for this poll yet";
        public string ButtonReopen() => "↩️ Re-open this poll";
        public string ButtonEditSubject() => "✏️ Subject";
        public string ButtonEditType() => "✏️ Type";
        public string ButtonAddOption() => "➕ Option";
        public string ButtonEditOption() => "✏️ Option";
        public string ButtonDeleteOption() => "➖ Option";
        public string ButtonShare() => "Share";
        public string ButtonClose() => "🚫 Close";
        public string ButtonClone() => "♻️ Clone";
        public string ButtonDelete() => "🗑 Delete";
        public string ButtonBackToList() => "🔙 Back to the list";
        public string ButtonRefresh() => "🔄 Refresh";
        public string PollMessageSelectOne() => "Select exactly one of the following options";
        public string PollMessageSelectMultiple() => "Select one or more of the following options";
        public string ConfirmClosePoll() => "Are you sure you want to close this poll?";
        public string ConfirmDeletePoll() => "Are you sure you want to delete this poll?";
        public string ConfirmClonePoll() => "Cloning will duplicate the poll and generates an identical poll without any votes. Are you sure you want to do this?";
        public string ConfirmYes() => "Ja";
        public string ConfirmNo() => "No";
        public string ButtonChooseSelectOne() => "Single select";
        public string ButtonChooseSelectMultiple() => "Multi-select";
        public string ErrorSomethingWentWrong() => "Something went wrong...";
        public string ErrorPollDoesNotExist() => "This poll does not exist anymore.";
        public string ButtonAddPoll() => "➕ Add a new poll";
    }
}
