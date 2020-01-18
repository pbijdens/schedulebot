using PB.ScheduleBot.API;
using PB.ScheduleBot.Model;
using PB.ScheduleBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.ScheduleBot.StateMachines
{
    public class UserSessionStateMachine : IUserSessionStateMachine
    {
        private readonly ITelegramAPI api;
        private readonly IUserStateRepository userStateRepository;
        private readonly IMessageService messageService;
        private readonly IPollRepository pollRepository;
        private readonly ILockProvider lockProvider;
        private readonly ILogger logger;

        public UserSessionStateMachine(ITelegramAPI api,
            IUserStateRepository userStateRepository,
            IMessageService messageService,
            IPollRepository pollRepository,
            ILockProvider lockProvider,
            ILogger logger)
        {
            this.api = api;
            this.userStateRepository = userStateRepository;
            this.messageService = messageService;
            this.pollRepository = pollRepository;
            this.lockProvider = lockProvider;
            this.logger = logger;
        }

        public async Task ProcessTextInputAsync(TelegramApiUser user, string message)
        {
            UserState state = await GetUserStateFor(user);
            switch (state.State)
            {
                case UserState.States.EditSubject:
                    await RunPollActionLockedAsync(user, state.ActivePollID, (poll) =>
                    {
                        poll.Subject = message;
                    });
                    await GotoStateAsync(user, UserState.States.EditActivePoll);
                    break;
                case UserState.States.AskForPollOptionNewName:
                    await RunPollActionLockedAsync(user, state.ActivePollID, (poll) =>
                    {
                        var option = poll.VoteOptions.Where(x => x.ID == state.ActionData).FirstOrDefault();
                        if (null != option)
                        {
                            option.Text = message;
                        }
                    });
                    await GotoStateAsync(user, UserState.States.EditActivePoll);
                    break;
                case UserState.States.AddVotingOption:
                    await RunPollActionLockedAsync(user, state.ActivePollID, (poll) =>
                    {
                        poll.VoteOptions.Add(new Poll.VoteOption
                        {
                            ID = shortid.ShortId.Generate(true, false, 12),
                            Text = message,
                            Votes = new List<TelegramApiUser>()
                        });
                    });
                    await GotoStateAsync(user, UserState.States.EditActivePoll);
                    break;
                default:
                    await api.SendMessageAsync(user.id, messageService.InputUnexpected());
                    break;
            }
        }

        public async Task CreateNewPollAsync(TelegramApiUser user)
        {
            logger.LogInformation("Creating new poll...");
            var sem = lockProvider.GetLockFor($"state-{user.id}");
            sem.WaitOne();
            try
            {
                logger.LogInformation("Obtained the lock for the user...");
                // Create a new poll
                var poll = await pollRepository.SaveAsync(new Poll());

                // Register the poll for the user
                UserState state = await GetUserStateFor(user);
                state.State = UserState.States.EditActivePoll;
                state.ActivePollID = poll.ID;
                state.OwnedPolls.Add(poll.ID);
                await userStateRepository.PutStateAsync(user, state);
            }
            finally
            {
                sem.Release();
            }
            await UpdateUserSessionChatAsync(user);
        }

        public async Task UpdateUserSessionChatAsync(TelegramApiUser user)
        {
            UserState state = await GetUserStateFor(user);
            switch (state.State)
            {
                case UserState.States.EditActivePoll:
                    await ShowEditActivePoll(user);
                    break;
                case UserState.States.ListOwnedPolls:
                    await ShowPollList(user);
                    break;
                case UserState.States.EditSubject:
                    await AskForSubject(user);
                    break;
                case UserState.States.SelectType:
                    await AskForType(user, state.ActivePollID);
                    break;
                case UserState.States.AddVotingOption:
                    await AskForNewPollOptionName(user);
                    break;
                case UserState.States.RemoveVotingOption:
                    await AskForVotingOptionToRemove(user, state);
                    break;
                case UserState.States.RenameVotingOption:
                    await AskForVotingOptionToRename(user, state);
                    break;
                case UserState.States.AskForPollOptionNewName:
                    await AskForPollOptionNewName(user);
                    break;
            }
        }

        private async Task AskForVotingOptionToRemove(TelegramApiUser user, UserState state)
        {
            var poll = await pollRepository.LoadAsync(state.ActivePollID);
            if (null == poll) return;

            if (poll.VoteOptions == null || poll.VoteOptions.Count == 0)
            {
                await GotoStateAsync(user, UserState.States.EditActivePoll);
            }
            else
            {
                List<TelegramApiInlineKeyboardButton> options = BuildOptionButtonList(state, poll, "remove-option");

                // send a message 
                await api.SendMessageAsync(user.id, messageService.EditQueryOptionToRemove(), "HTML", null, null, null, new TelegramApiInlineKeyboardMarkup
                {
                    inline_keyboard = options.Select(x => new TelegramApiInlineKeyboardButton[] { x }).ToArray()
                });
            }
        }

        private async Task AskForVotingOptionToRename(TelegramApiUser user, UserState state)
        {
            var poll = await pollRepository.LoadAsync(state.ActivePollID);
            if (null == poll) return;

            if (poll.VoteOptions == null || poll.VoteOptions.Count == 0)
            {
                await GotoStateAsync(user, UserState.States.EditActivePoll);
            }
            else
            {
                List<TelegramApiInlineKeyboardButton> options = BuildOptionButtonList(state, poll, "rename-option");

                // send a message 
                await api.SendMessageAsync(user.id, messageService.EditQueryOptionToRename(), "HTML", null, null, null, new TelegramApiInlineKeyboardMarkup
                {
                    inline_keyboard = options.Select(x => new TelegramApiInlineKeyboardButton[] { x }).ToArray()
                });
            }
        }

        private static List<TelegramApiInlineKeyboardButton> BuildOptionButtonList(UserState state, Poll poll, string command)
        {
            int i = 1;
            List<TelegramApiInlineKeyboardButton> options = new List<TelegramApiInlineKeyboardButton>();
            foreach (var option in poll.VoteOptions)
            {
                options.Add(new TelegramApiInlineKeyboardButton
                {
                    callback_data = $"edit.{state.ActivePollID}.{command}.{option.ID}",
                    text = $"{i++}: {option.Text}"
                });
            }

            return options;
        }

        private async Task AskForType(TelegramApiUser user, string pollID)
        {
            await api.SendMessageAsync(user.id, messageService.EditQueryPollType(), "HTML", null, null, null, new TelegramApiInlineKeyboardMarkup
            {
                inline_keyboard = new TelegramApiInlineKeyboardButton[][] {
                    new TelegramApiInlineKeyboardButton[] {
                        new TelegramApiInlineKeyboardButton {
                            callback_data = $"edit.{pollID}.set-type.{(int)Poll.PollType.Single}",
                            text = "Select one"
                        },
                        new TelegramApiInlineKeyboardButton {
                            callback_data = $"edit.{pollID}.set-type.{(int)Poll.PollType.Multiple}",
                            text = "Select multiple"
                        },
                    }
                }
            });
        }

        private async Task AskForSubject(TelegramApiUser user)
        {
            await api.SendMessageAsync(user.id, messageService.EditQueryPollSubject());
        }
        private async Task AskForPollOptionNewName(TelegramApiUser user)
        {
            await api.SendMessageAsync(user.id, messageService.EditQueryPollOptionNewName());
        }

        private async Task AskForNewPollOptionName(TelegramApiUser user)
        {
            await api.SendMessageAsync(user.id, messageService.EditQueryNewPollOptionName());
        }

        public async Task GotoShowListStateAsync(TelegramApiUser user)
        {
            var sem = lockProvider.GetLockFor($"state-{user.id}");
            sem.WaitOne();
            try
            {
                UserState state = await GetUserStateFor(user);
                state.State = UserState.States.ListOwnedPolls;
                await userStateRepository.PutStateAsync(user, state);
            }
            finally
            {
                sem.Release();
            }
            await UpdateUserSessionChatAsync(user);
        }

        public async Task ProcessQueryCallbackAsync(TelegramApiCallbackQuery callback)
        {
            if (callback.data.StartsWith("open."))
            {
                bool success = await OpenPoll(callback.from, callback.data.Substring(5));
                if (success)
                {
                    await api.AnswerCallbackQuery(callback.id);
                }
                else
                {
                    await api.AnswerCallbackQuery(callback.id, "This poll does not exist anymore.", true);
                }
                await UpdateUserSessionChatAsync(callback.from);
            }
            else if (callback.data.StartsWith("edit."))
            {
                string pollID = callback.data.Split('.')[1];
                string command = callback.data.Split('.')[2];
                var poll = await pollRepository.LoadAsync(pollID);
                if (null != poll)
                {
                    switch (command)
                    {
                        case "subject": await GotoStateAsync(callback.from, UserState.States.EditSubject); break;
                        case "type": await GotoStateAsync(callback.from, UserState.States.SelectType); break;
                        case "add-voting-option": await GotoStateAsync(callback.from, UserState.States.AddVotingOption); break;
                        case "remove-voting-option": await GotoStateAsync(callback.from, UserState.States.RemoveVotingOption); break;
                        case "rename-voting-option": await GotoStateAsync(callback.from, UserState.States.RenameVotingOption); break;
                        case "set-type":
                            await RunPollActionLockedAsync(callback.from, poll.ID, (p) =>
                            {
                                if (!int.TryParse(callback.data.Split('.')[3], out int type)) { type = 0; }
                                p.Type = (Poll.PollType)type;
                            });
                            await GotoStateAsync(callback.from, UserState.States.EditActivePoll);
                            break;
                        case "remove-option":
                            await RunPollActionLockedAsync(callback.from, poll.ID, (p) =>
                            {
                                string optionID = callback.data.Split('.')[3];
                                p.VoteOptions.RemoveAll(x => x.ID == optionID);
                            });
                            await GotoStateAsync(callback.from, UserState.States.EditActivePoll);
                            break;
                        case "rename-option":
                            await GotoStateAsync(callback.from, UserState.States.AskForPollOptionNewName, (state) => state.ActionData = callback.data.Split('.')[3]);
                            break;
                        default: break; // whatever
                    }

                    await api.AnswerCallbackQuery(callback.id);
                }
                else
                {
                    await api.AnswerCallbackQuery(callback.id, "This poll does not exist anymore.", true);
                }
            }
        }

        private async Task RunPollActionLockedAsync(TelegramApiUser user, string pollID, Action<Poll> action)
        {
            var sem = lockProvider.GetLockFor($"`poll-{pollID}");
            sem.WaitOne();
            try
            {
                var poll = await pollRepository.LoadAsync(pollID);
                if (null != poll)
                {
                    action(poll);
                }
                await pollRepository.SaveAsync(poll);
            }
            finally
            {
                sem.Release();
            }
        }

        private async Task<bool> OpenPoll(TelegramApiUser user, string id)
        {
            var poll = await pollRepository.LoadAsync(id);
            if (null == poll)
            {
                return false;
            }
            else
            {
                var sem = lockProvider.GetLockFor($"state-{user.id}");
                sem.WaitOne();
                try
                {
                    UserState state = await GetUserStateFor(user);
                    state.State = UserState.States.EditActivePoll;
                    state.ActivePollID = id;
                    await userStateRepository.PutStateAsync(user, state);
                    return true;
                }
                finally
                {
                    sem.Release();
                }
            }
        }

        private async Task<UserState> GetUserStateFor(TelegramApiUser user)
        {
            UserState state = await userStateRepository.GetStateAsync(user);
            if (null == state)
            {
                state = new UserState();
            }
            if (null == state.OwnedPolls)
            {
                state.OwnedPolls = new List<string>();
            }

            return state;
        }

        private async Task ShowEditActivePoll(TelegramApiUser user)
        {
            UserState state = await userStateRepository.GetStateAsync(user);
            if (null == state || string.IsNullOrWhiteSpace(state.ActivePollID))
            {
                await api.SendMessageAsync(user.id, messageService.ThereIsNoActivePollToEdit());
                return;
            }
            Poll poll = await pollRepository.LoadAsync(state.ActivePollID);
            if (null == poll)
            {
                await api.SendMessageAsync(user.id, messageService.ThereIsNoActivePollToEdit());
                return;
            }

            StringBuilder messageBuilder = new StringBuilder();

            messageBuilder.AppendLine($"<b>{HtmlEscape(poll.Subject)}</b>");
            messageBuilder.AppendLine($"<i>{poll.Type}</i>");
            messageBuilder.AppendLine($"");
            if (null == poll.VoteOptions || poll.VoteOptions.Count == 0)
            {
                messageBuilder.AppendLine($"Vote options have not been set up for this poll yet.");
            }
            else
            {
                foreach (var voteOption in poll.VoteOptions ?? new List<Poll.VoteOption>())
                {
                    messageBuilder.AppendLine($"<b>{HtmlEscape(voteOption.Text)} ({voteOption.Votes?.Count ?? 0})</b>");
                    if (null != voteOption.Votes && voteOption.Votes.Count > 0)
                    {
                        foreach (var vote in voteOption.Votes)
                        {
                            messageBuilder.AppendLine($" - {ShortName(vote)}");
                        }
                        messageBuilder.AppendLine($"");
                    }
                    else
                    {
                        messageBuilder.AppendLine($"<i>No votes.</i>");

                    }
                }
            }
            messageBuilder.AppendLine($"");
            messageBuilder.AppendLine($"Votes:");
            messageBuilder.AppendLine($"<i>{HtmlEscape(poll.ID)}</i>");
            messageBuilder.AppendLine($"<i>{DateTime.UtcNow.Ticks}</i>");
            // TODO: ADD BUTTONS TO EDIT THE WRETCHED POLL

            TelegramApiInlineKeyboardMarkup markup = null;
            if (!poll.IsClosed)
            {
                markup = new TelegramApiInlineKeyboardMarkup
                {
                    inline_keyboard = new TelegramApiInlineKeyboardButton[][] {
                        new TelegramApiInlineKeyboardButton[] { new TelegramApiInlineKeyboardButton {
                            callback_data = $"edit.{poll.ID}.subject",
                            text = "Edit subject",
                        } },
                        new TelegramApiInlineKeyboardButton[] { new TelegramApiInlineKeyboardButton {
                            callback_data = $"edit.{poll.ID}.type",
                            text = "Choose type",
                        } },
                        new TelegramApiInlineKeyboardButton[] { new TelegramApiInlineKeyboardButton {
                            callback_data = $"edit.{poll.ID}.add-voting-option",
                            text = "Add voting option",
                        } },
                        new TelegramApiInlineKeyboardButton[] { new TelegramApiInlineKeyboardButton {
                            callback_data = $"edit.{poll.ID}.remove-voting-option",
                            text = "Remove voting option",
                        } },
                        new TelegramApiInlineKeyboardButton[] { new TelegramApiInlineKeyboardButton {
                            callback_data = $"edit.{poll.ID}.rename-voting-option",
                            text = "Rename voting option",
                        } },
                    }
                };
            }
            await api.SendMessageAsync(user.id, messageBuilder.ToString(), "HTML", null, null, null, markup);
        }

        private object ShortName(TelegramApiUser user)
        {
            if (!string.IsNullOrWhiteSpace(user.username))
            {
                return HtmlEscape(user.username);
            }
            else
            {
                return HtmlEscape($"{user.first_name} {user.last_name}");
            }
        }

        private object HtmlEscape(string subject)
        {
            // TODO: IMPLEMENT
            return subject.Replace("<", "").Replace(">", "");
        }

        private async Task ShowPollList(TelegramApiUser user)
        {
            UserState state = await GetUserStateFor(user);

            List<Poll> activePolls = new List<Poll>();
            foreach (var id in state?.OwnedPolls ?? new List<string>())
            {
                Poll poll = await pollRepository.LoadAsync(id);
                if (null != poll && !poll.IsClosed)
                {
                    activePolls.Add(poll);
                }
            }

            if (null == state || activePolls.Count == 0)
            {
                await api.SendMessageAsync(user.id, messageService.YouHaveNoPolls());
            }
            else
            {
                List<TelegramApiInlineKeyboardButton[]> rows = new List<TelegramApiInlineKeyboardButton[]>();
                int i = 1;
                foreach (var poll in activePolls)
                {
                    rows.Add(new TelegramApiInlineKeyboardButton[] {
                        new TelegramApiInlineKeyboardButton
                        {
                            callback_data = $"open.{poll.ID}",
                            text = $"{i++}: {poll.Subject}",
                        }
                    });
                }
                TelegramApiInlineKeyboardMarkup markup = new TelegramApiInlineKeyboardMarkup
                {
                    inline_keyboard = rows.ToArray()
                };
                await api.SendMessageAsync(user.id, messageService.HereAreYourThisManyPolls(activePolls.Count), "HTML", null, null, null, markup);
            }

        }

        public async Task GotoStateAsync(TelegramApiUser user, UserState.States newState, Action<UserState> alsoDoThis = null)
        {
            var sem = lockProvider.GetLockFor($"state-{user.id}");
            sem.WaitOne();
            try
            {
                UserState state = await GetUserStateFor(user);
                state.State = newState;
                if (null != alsoDoThis) alsoDoThis(state);
                await userStateRepository.PutStateAsync(user, state);
            }
            finally
            {
                sem.Release();
            }
            await UpdateUserSessionChatAsync(user);
        }

    }
}
