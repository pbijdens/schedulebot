namespace PB.ScheduleBot.Services
{
    public interface IMessageService
    {
        string CommandNotSupported(string command);
        string InputUnexpected();
        string Help();
        string YouHaveNoPolls();
        string HereAreYourThisManyPolls(int count);
        string ThereIsNoActivePollToEdit();
        string EditQueryPollSubject();
        string EditQueryPollType();
        string EditQueryOptionToRemove();
        string EditQueryOptionToRename();
        string EditQueryPollOptionNewName();
        string EditQueryNewPollOptionName();
        string PollMessageNoSubjectText();
        string PollMessagePollIsClosed();
        string PollMessagePollHasNoVotingOptions();
        string ButtonReopen();
        string ButtonEditSubject();
        string ButtonEditType();
        string ButtonAddOption();
        string ButtonEditOption();
        string ButtonDeleteOption();
        string ButtonShare();
        string ButtonClose();
        string ButtonClone();
        string ButtonDelete();
        string ButtonBackToList();
        string ButtonRefresh();
        string PollMessageSelectOne();
        string PollMessageSelectMultiple();
        string ConfirmClosePoll();
        string ConfirmDeletePoll();
        string ConfirmClonePoll();
        string ConfirmYes();
        string ConfirmNo();
        string ButtonChooseSelectOne();
        string ButtonChooseSelectMultiple();
        string ErrorSomethingWentWrong();
        string ErrorPollDoesNotExist();
        string ButtonAddPoll();
    }
}