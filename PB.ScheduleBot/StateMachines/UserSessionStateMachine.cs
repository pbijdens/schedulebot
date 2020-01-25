using PB.ScheduleBot.API;
using PB.ScheduleBot.Model;
using PB.ScheduleBot.Services;
using PB.ScheduleBot.Utils;
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

        public async Task ProcessTextInputAsync(TelegramApiUser user, long messageID, string message)
        {
            await DeletePromptsAsync(user);
            UserState state = await GetUserStateFor(user);
            logger.LogInformation($"Processing text input for user {user.id}'s in session state state {state.State}.");
            switch (state.State)
            {
                case UserState.States.EditSubject:
                    await RunPollActionLockedAsync(user, state.ActivePollID, (poll) =>
                    {
                        poll.Subject = message;
                    });
                    await api.DeleteMessageForChatAsync(user.id, messageID); // delete the user input
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
                    await api.DeleteMessageForChatAsync(user.id, messageID); // delete the user input
                    await GotoStateAsync(user, UserState.States.EditActivePoll);
                    break;
                case UserState.States.AddVotingOption:
                    await RunPollActionLockedAsync(user, state.ActivePollID, (poll) =>
                    {
                        poll.VoteOptions.Add(new Poll.VoteOption
                        {
                            Text = message,
                            Votes = new List<TelegramApiUser>()
                        });
                    });
                    await api.DeleteMessageForChatAsync(user.id, messageID); // delete the user input
                    await GotoStateAsync(user, UserState.States.EditActivePoll);
                    break;
                default:
                    await ResetPrivateMessageHistory(user);
                    await api.SendMessageAsync(user.id, messageService.InputUnexpected());
                    break;
            }
        }

        public async Task ResetPrivateMessageHistory(TelegramApiUser user)
        {
            await RunStateActionLockedAsync(user, st =>
            {
                // There was text input that we could not process, so we do not update or delete the last sent messages we simply start from scratch
                st.LastSentConfirmationMessage = null;
                st.LastSentStateMessage = null;
                st.RegisteredPrompts = new List<TelegramApiMessage>();
            });
        }

        public async Task UpdateUserChatSessionForStateAsync(TelegramApiUser user)
        {
            UserState state = await GetUserStateFor(user);
            logger.LogInformation($"Updating user {user.id}'s chat session for state {state.State}.");
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
                case UserState.States.ClosePoll:
                    await AskForConfirmation(messageService.ConfirmClosePoll(), "close", user, state.ActivePollID);
                    break;
                case UserState.States.DeletePoll:
                    await AskForConfirmation(messageService.ConfirmDeletePoll(), "delete", user, state.ActivePollID);
                    break;
                case UserState.States.ClonePoll:
                    await AskForConfirmation(messageService.ConfirmClonePoll(), "clone", user, state.ActivePollID);
                    break;
            }
        }

        private async Task AskForConfirmation(string message, string command, TelegramApiUser user, string pollID)
        {
            var confirmationMessage = await api.SendMessageAsync(user.id, message.HtmlSafe(), "HTML", null, null, null, new TelegramApiInlineKeyboardMarkup
            {
                inline_keyboard = new TelegramApiInlineKeyboardButton[][] {
                    new TelegramApiInlineKeyboardButton[] {
                        new TelegramApiInlineKeyboardButton {
                            callback_data = $"edit.{pollID}.confirmed-{command}.true",
                            text = messageService.ConfirmYes()
                        },
                        new TelegramApiInlineKeyboardButton {
                            callback_data = $"edit.{pollID}.confirmed-{command}.false",
                            text = messageService.ConfirmNo()
                        },
                    }
                }
            });
            await RunStateActionLockedAsync(user, (state) => state.LastSentConfirmationMessage = confirmationMessage);
        }

        public async Task CreateNewPollAsync(TelegramApiUser user)
        {
            logger.LogInformation($"Creating a new poll for user {user.id}'s.");
            var sem = lockProvider.GetLockFor($"state-{user.id}");
            sem.WaitOne();
            try
            {
                logger.LogInformation($"Obtained lock for user {user.id}'s.");
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
            await UpdateUserChatSessionForStateAsync(user);
        }

        public async Task<Poll> ClonePollAsync(TelegramApiUser user, Poll sourcePoll)
        {
            logger.LogInformation($"Cloning poll for user {user.id}.");

            if (null == sourcePoll)
            {
                sourcePoll = new Poll();
            }

            Poll result = null;
            var sem = lockProvider.GetLockFor($"state-{user.id}");
            sem.WaitOne();
            try
            {
                result = new Poll();
                result.Subject = sourcePoll.Subject;
                result.VoteOptions = sourcePoll.VoteOptions.Select(x => new Poll.VoteOption
                {
                    Text = x.Text,
                    Votes = new List<TelegramApiUser>()
                }).ToList();
                await pollRepository.SaveAsync(result);

                // Register the poll for the user
                UserState state = await GetUserStateFor(user);
                state.State = UserState.States.EditActivePoll;
                state.ActivePollID = result.ID;
                state.OwnedPolls.Add(result.ID);
                await userStateRepository.PutStateAsync(user, state);
            }
            finally
            {
                sem.Release();
            }
            await UpdateUserChatSessionForStateAsync(user);
            return result;
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
                var sentMessage = await api.SendMessageAsync(user.id, messageService.EditQueryOptionToRemove(), "HTML", null, null, null, new TelegramApiInlineKeyboardMarkup
                {
                    inline_keyboard = options.Select(x => new TelegramApiInlineKeyboardButton[] { x }).ToArray()
                });
                await RegisterPromptAsync(user, sentMessage);
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
                var sentMessage = await api.SendMessageAsync(user.id, messageService.EditQueryOptionToRename(), "HTML", null, null, null, new TelegramApiInlineKeyboardMarkup
                {
                    inline_keyboard = options.Select(x => new TelegramApiInlineKeyboardButton[] { x }).ToArray()
                });
                await RegisterPromptAsync(user, sentMessage);
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
            var sentMessage = await api.SendMessageAsync(user.id, messageService.EditQueryPollType(), "HTML", null, null, null, new TelegramApiInlineKeyboardMarkup
            {
                inline_keyboard = new TelegramApiInlineKeyboardButton[][] {
                    new TelegramApiInlineKeyboardButton[] {
                        new TelegramApiInlineKeyboardButton {
                            callback_data = $"edit.{pollID}.set-type.{(int)Poll.PollType.Single}",
                            text = messageService.ButtonChooseSelectOne()
                        },
                        new TelegramApiInlineKeyboardButton {
                            callback_data = $"edit.{pollID}.set-type.{(int)Poll.PollType.Multiple}",
                            text = messageService.ButtonChooseSelectMultiple()
                        },
                    }
                }
            });
            await RegisterPromptAsync(user, sentMessage);
        }

        private async Task AskForSubject(TelegramApiUser user)
        {
            var sentMessage = await api.SendMessageAsync(user.id, messageService.EditQueryPollSubject());
            await RegisterPromptAsync(user, sentMessage);
        }

        private async Task AskForPollOptionNewName(TelegramApiUser user)
        {
            var sentMessage = await api.SendMessageAsync(user.id, messageService.EditQueryPollOptionNewName());
            await RegisterPromptAsync(user, sentMessage);
        }

        private async Task AskForNewPollOptionName(TelegramApiUser user)
        {
            var sentMessage = await api.SendMessageAsync(user.id, messageService.EditQueryNewPollOptionName());
            await RegisterPromptAsync(user, sentMessage);
        }

        public async Task GotoShowListStateAsync(TelegramApiUser user)
        {
            var sem = lockProvider.GetLockFor($"state-{user.id}");
            sem.WaitOne();
            try
            {
                logger.LogInformation($"Obtained lock for user {user.id}'s.");
                UserState state = await GetUserStateFor(user);
                state.State = UserState.States.ListOwnedPolls;
                await userStateRepository.PutStateAsync(user, state);
            }
            finally
            {
                sem.Release();
            }
            await UpdateUserChatSessionForStateAsync(user);
        }

        public async Task ProcessCallbackQueryAsync(TelegramApiCallbackQuery callback)
        {
            await DeletePromptsAsync(callback.from);
            await DeleteConfirmationMessageIfAnyAsync(callback.from);

            string source = callback.data.GetQueryPart(0);

            switch (source)
            {
                case "open":
                    await ProcessOpenCallbackQueryAsync(callback);
                    break;
                case "edit":
                    await ProcessEditCallbackQueryAsync(callback);
                    break;
                case "list":
                    await ProcessListCallbackQueryAsync(callback);
                    break;
                case "new":
                    await ProcessNewCallbackQueryAsync(callback);
                    break;
                default:
                    await api.AnswerCallbackQuery(callback.id, messageService.ErrorSomethingWentWrong(), true);
                    logger.LogError($"Callback query '{callback.data}' is not supported.");
                    break;
            }
        }

        private async Task DeleteConfirmationMessageIfAnyAsync(TelegramApiUser from)
        {
            var state = await userStateRepository.GetStateAsync(from);
            if (state.LastSentConfirmationMessage != null)
            {
                await RunStateActionLockedAsync(from, x => x.LastSentConfirmationMessage = null);
                await api.DeleteMessageForChatAsync(from.id, state.LastSentConfirmationMessage.message_id);
            }
        }

        private async Task ProcessListCallbackQueryAsync(TelegramApiCallbackQuery callback)
        {
            await GotoStateAsync(callback.from, UserState.States.ListOwnedPolls, (state) => state.ActivePollID = null);
            await api.AnswerCallbackQuery(callback.id);
        }

        private async Task ProcessNewCallbackQueryAsync(TelegramApiCallbackQuery callback)
        {
            await CreateNewPollAsync(callback.from);
        }

        private async Task ProcessEditCallbackQueryAsync(TelegramApiCallbackQuery callback)
        {
            string pollID = callback.data.GetQueryPart(1);
            string command = callback.data.GetQueryPart(2);
            var poll = await pollRepository.LoadAsync(pollID);
            if (null != poll)
            {
                await ProcessCallbackQueryForEditing(callback, command, poll);

                await api.AnswerCallbackQuery(callback.id);
            }
            else
            {
                logger.LogError($"Callback query '{callback.data}' leads to non-existing poll.");
                await api.AnswerCallbackQuery(callback.id, messageService.ErrorPollDoesNotExist(), true);
            }
        }

        private async Task ProcessOpenCallbackQueryAsync(TelegramApiCallbackQuery callback)
        {
            string pollID = callback.data.GetQueryPart(1);
            bool success = await OpenPoll(callback.from, pollID);
            if (success)
            {
                await api.AnswerCallbackQuery(callback.id);
            }
            else
            {
                await api.AnswerCallbackQuery(callback.id, messageService.ErrorPollDoesNotExist(), true);
            }
            await UpdateUserChatSessionForStateAsync(callback.from);
        }

        private async Task ProcessCallbackQueryForEditing(TelegramApiCallbackQuery callback, string command, Poll poll)
        {
            switch (command)
            {
                case "subject": await GotoStateAsync(callback.from, UserState.States.EditSubject); break;
                case "type": await GotoStateAsync(callback.from, UserState.States.SelectType); break;
                case "add-voting-option": await GotoStateAsync(callback.from, UserState.States.AddVotingOption); break;
                case "remove-voting-option": await GotoStateAsync(callback.from, UserState.States.RemoveVotingOption); break;
                case "rename-voting-option": await GotoStateAsync(callback.from, UserState.States.RenameVotingOption); break;
                case "delete": await GotoStateAsync(callback.from, UserState.States.DeletePoll); break;
                case "close": await GotoStateAsync(callback.from, UserState.States.ClosePoll); break;
                case "clone": await GotoStateAsync(callback.from, UserState.States.ClonePoll); break;
                case "set-type":
                    {
                        string typeString = callback.data.GetQueryPart(3);
                        await RunPollActionLockedAsync(callback.from, poll.ID, (p) =>
                        {
                            if (!int.TryParse(typeString, out int type)) { type = 0; }
                            p.Type = (Poll.PollType)type;
                        });
                        await GotoStateAsync(callback.from, UserState.States.EditActivePoll);
                    }
                    break;
                case "remove-option":
                    {
                        string optionID = callback.data.GetQueryPart(3);
                        await RunPollActionLockedAsync(callback.from, poll.ID, (p) =>
                        {
                            p.VoteOptions.RemoveAll(x => x.ID == optionID);
                        });
                    }
                    await GotoStateAsync(callback.from, UserState.States.EditActivePoll);
                    break;
                case "rename-option":
                    {
                        string optionID = callback.data.GetQueryPart(3);
                        await GotoStateAsync(callback.from, UserState.States.AskForPollOptionNewName, (state) => state.ActionData = optionID);
                    }
                    break;
                case "confirmed-close":
                    {
                        string choice = callback.data.GetQueryPart(3);
                        await RunPollActionLockedAsync(callback.from, poll.ID, (p) =>
                        {
                            if (bool.TryParse(choice, out bool confirmed) && confirmed) { p.IsClosed = true; }
                        });
                    }
                    await GotoStateAsync(callback.from, UserState.States.ListOwnedPolls, (state) => state.ActivePollID = null);
                    break;
                case "confirmed-delete":
                    {
                        string choice = callback.data.GetQueryPart(3);
                        await RunPollActionLockedAsync(callback.from, poll.ID, (p) =>
                        {
                            if (bool.TryParse(choice, out bool confirmed) && confirmed) { p.IsDeleted = true; }
                        });
                        await GotoStateAsync(callback.from, UserState.States.ListOwnedPolls, (state) => state.ActivePollID = null);
                    }
                    break;
                case "confirmed-clone":
                    {
                        string choice = callback.data.GetQueryPart(3);
                        if (bool.TryParse(choice, out bool confirmed) && confirmed)
                        {
                            Poll newPoll = await ClonePollAsync(callback.from, poll);
                            await GotoStateAsync(callback.from, UserState.States.EditActivePoll, (state) => state.ActivePollID = newPoll?.ID);
                        }
                        else
                        {
                            await GotoStateAsync(callback.from, UserState.States.ListOwnedPolls, (state) => state.ActivePollID = null);
                        }
                    }
                    break;
                case "reopen":
                    await RunPollActionLockedAsync(callback.from, poll.ID, (p) =>
                    {
                        p.IsClosed = false;
                    });
                    await GotoStateAsync(callback.from, UserState.States.ListOwnedPolls, (state) => state.ActivePollID = null);
                    break;
                default: break; // whatever
            }
        }

        private async Task RunPollActionLockedAsync(TelegramApiUser user, string pollID, Action<Poll> action)
        {
            var sem = lockProvider.GetLockFor($"`poll-{pollID}");
            sem.WaitOne();
            try
            {
                logger.LogInformation($"Obtained lock for user {user.id}'s.");
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
                    logger.LogInformation($"Obtained lock for user {user.id}'s.");
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
                // We should not have been in this state...
                await api.SendMessageAsync(user.id, messageService.ThereIsNoActivePollToEdit());
                return;
            }
            Poll poll = await pollRepository.LoadAsync(state.ActivePollID);
            if (null == poll)
            {
                // We should not have been in this state
                await api.SendMessageAsync(user.id, messageService.ThereIsNoActivePollToEdit());
                return;
            }

            string messageBuilder = poll.ConstructMessageText(messageService);

            TelegramApiInlineKeyboardMarkup markup = poll.ConstructInlineKeyboard(messageService);
            if (null == state.LastSentStateMessage)
            {
                var currentMessage = await api.SendMessageAsync(user.id, messageBuilder, "HTML", null, null, null, markup);
                await RunStateActionLockedAsync(user, st =>
                {
                    st.LastSentStateMessage = currentMessage;
                });
            }
            else
            {
                await api.EditMessageTextAsync(user.id, $"{state.LastSentStateMessage.message_id}", messageBuilder, "HTML", null, markup);
            }
        }

        private async Task ShowPollList(TelegramApiUser user)
        {
            UserState state = await GetUserStateFor(user);

            List<Poll> activePolls = new List<Poll>();
            foreach (var id in state?.OwnedPolls ?? new List<string>())
            {
                Poll poll = await pollRepository.LoadAsync(id);
                if (null != poll && !poll.IsDeleted)
                {
                    activePolls.Add(poll);
                }
            }

            if (null == state || activePolls.Count == 0)
            {
                var sentMessage = await api.SendMessageAsync(user.id, messageService.YouHaveNoPolls());
                await RegisterPromptAsync(user, sentMessage);
            }
            else
            {
                List<TelegramApiInlineKeyboardButton[]> rows = new List<TelegramApiInlineKeyboardButton[]>();
                int i = 1;
                foreach (var poll in activePolls.OrderByDescending(x => x.IsClosed).ThenBy(x => x.ModificationDate))
                {
                    rows.Add(new TelegramApiInlineKeyboardButton[] {
                        new TelegramApiInlineKeyboardButton
                        {
                            callback_data = $"open.{poll.ID}",
                            text = $"{i++}: {poll.AsListEntry()}",
                        }
                    });
                }
                rows.Add(new TelegramApiInlineKeyboardButton[] {
                        new TelegramApiInlineKeyboardButton
                        {
                            callback_data = $"new",
                            text = messageService.ButtonAddPoll(),
                        }
                    });
                TelegramApiInlineKeyboardMarkup markup = new TelegramApiInlineKeyboardMarkup
                {
                    inline_keyboard = rows.ToArray()
                };
                var sentMessage = await api.SendMessageAsync(user.id, messageService.HereAreYourThisManyPolls(activePolls.Count), "HTML", null, null, null, markup);
                await RegisterPromptAsync(user, sentMessage);
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
            await UpdateUserChatSessionForStateAsync(user);
        }

        public async Task RunStateActionLockedAsync(TelegramApiUser user, Action<UserState> action = null)
        {
            var sem = lockProvider.GetLockFor($"state-{user.id}");
            sem.WaitOne();
            try
            {
                UserState state = await GetUserStateFor(user);
                if (null != action) action(state);
                await userStateRepository.PutStateAsync(user, state);
            }
            finally
            {
                sem.Release();
            }
        }

        private async Task DeletePromptsAsync(TelegramApiUser user)
        {
            UserState state = await GetUserStateFor(user);
            foreach (var prompt in state?.RegisteredPrompts ?? new List<TelegramApiMessage>())
            {
                await api.DeleteMessageForChatAsync(user.id, prompt.message_id);
            }
            await RunStateActionLockedAsync(user, (st) => st.RegisteredPrompts?.Clear());
        }

        private async Task RegisterPromptAsync(TelegramApiUser user, TelegramApiMessage message)
        {
            await RunStateActionLockedAsync(user, (st) =>
            {
                if (null == st.RegisteredPrompts) st.RegisteredPrompts = new List<TelegramApiMessage>();
                st.RegisteredPrompts.Add(message);
            });

        }

    }
}
