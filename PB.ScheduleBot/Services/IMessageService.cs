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
    }
}