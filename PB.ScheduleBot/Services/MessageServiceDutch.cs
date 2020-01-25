using System;
using System.Collections.Generic;
using System.Text;

namespace PB.ScheduleBot.Services
{
    public class MessageServiceDutch : IMessageService
    {
        public string CommandNotSupported(string command) => $"Het commando <b>{command}</b> wordt niet ondersteund. Gebruik /help om om hulp te vragen..";
        public string InputUnexpected() => $"Dat snap ik niet. {Help()}";
        public string Help() => $"Gebruik /list om je afspraken te bekijken. Gebruik /new om een nieuwe afspraak te maken.";
        public string YouHaveNoPolls() => $"Je hebt nog geen afspraken.";
        public string HereAreYourThisManyPolls(int count) => $"Je hebt op dit moment {count} afspraken:";
        public string ThereIsNoActivePollToEdit() => $"Gebruik eerst /new om een nieuwe afspraak te maken.";
        public string EditQueryPollSubject() => $"Hoe moet ik deze afspraak noemen?";
        public string EditQueryPollType() => $"Mogen gebruikers slechts een enkele keuze maken, of mogen ze meerdere mogelijkheden opgeven?";
        public string EditQueryOptionToRemove() => $"Welke keuzemogelijkheid moet verwijderd worden?";
        public string EditQueryOptionToRename() => $"Welke keuzemogelijkheid moet hernoemd worden?";
        public string EditQueryPollOptionNewName() => "Wat is de nieuwe naam voor deze keuzemogelijkheid?";
        public string EditQueryNewPollOptionName() => "Wat is de naam voor deze keuzemogelijkheid?";
        public string PollMessageNoSubjectText() => "Deze afspraak heeft nog geen omschrijving.";
        public string PollMessagePollIsClosed() => "Deze afspraak is afgesloten en er kan niet meer gestemd worden.";
        public string PollMessagePollHasNoVotingOptions() => "Deze afspraak is nog niet volledig geconfigureerd.";
        public string ButtonReopen() => "↩️ Heropenen";
        public string ButtonEditSubject() => "✏️ Onderwerp";
        public string ButtonEditType() => "✏️ Type";
        public string ButtonAddOption() => "➕ Keuze";
        public string ButtonEditOption() => "✏️ Keuze";
        public string ButtonDeleteOption() => "➖ Keuze";
        public string ButtonShare() => "Delen";
        public string ButtonClose() => "🚫 Afsluiten";
        public string ButtonClone() => "♻️ Dupliceren";
        public string ButtonDelete() => "🗑 Verwijderen";
        public string ButtonBackToList() => "🔙 Terug naar de lijst";
        public string ButtonRefresh() => "🔄 Verversen";
        public string PollMessageSelectOne() => "Kies een van de volgende mogelijkheden";
        public string PollMessageSelectMultiple() => "Kies een of meer van de volgende mogelijkheden";
        public string ConfirmClosePoll() => "Weet je zeker dat je deze afspraak wil afsluiten?";
        public string ConfirmDeletePoll() => "Weet je zeker dat je deze afspraak wil verwijderen?";
        public string ConfirmClonePoll() => "Er wordt een kopie van deze afspraak gemaakt. Weet je zeker dat je dat wil doen?";
        public string ConfirmYes() => "Ja";
        public string ConfirmNo() => "Nee";
        public string ButtonChooseSelectOne() => "Kies één";
        public string ButtonChooseSelectMultiple() => "Kies meerdere";
        public string ErrorSomethingWentWrong() => "Iets is niet goed gegaan...";
        public string ErrorPollDoesNotExist() => "Deze afspraak bestaat niet (meer).";
        public string ButtonAddPoll() => "➕ Nieuwe afspraak toevoegen";
    }
}
