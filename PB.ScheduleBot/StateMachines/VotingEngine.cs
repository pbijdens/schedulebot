using PB.ScheduleBot.API;
using PB.ScheduleBot.Model;
using PB.ScheduleBot.Services;
using PB.ScheduleBot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PB.ScheduleBot.StateMachines
{

    public class VotingEngine : IVotingEngine // Singleton
    {
        private readonly ITelegramAPI api;
        private readonly IUserStateRepository userStateRepository;
        private readonly IMessageService messageService;
        private readonly IPollRepository pollRepository;
        private readonly ILockProvider lockProvider;
        private readonly ILogger logger;

        public VotingEngine(ITelegramAPI api,
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

        public async Task ProcessCallbackQueryAsync(TelegramApiCallbackQuery callback)
        {
            string command = callback.data.GetQueryPart(0);
            if (command == "vote")
            {
                await OnVoteCommand(callback);
            }
            else if (command == "refresh")
            {
                await OnRefreshCommand(callback);

            }
        }

        private async Task OnRefreshCommand(TelegramApiCallbackQuery callback)
        {
            logger.LogInformation($"Refresh {callback.data} requested by {callback.from.id}/{callback.from.username}/{callback.from.last_name}");

            string pollID = callback.data.GetQueryPart(1);
            await Refresh(callback.inline_message_id, pollID);
        }

        private async Task OnVoteCommand(TelegramApiCallbackQuery callback)
        {
            logger.LogInformation($"Vote {callback.data} from {callback.from.id}/{callback.from.username}/{callback.from.last_name}");

            string pollID = callback.data.GetQueryPart(1);
            string optionID = callback.data.GetQueryPart(3);

            var sem = lockProvider.GetLockFor($"poll-{pollID}");
            sem.WaitOne();
            try
            {
                var poll = await pollRepository.LoadAsync(pollID);
                if (null != poll)
                {
                    await ProcessVoteActionWhileLocked(callback, callback.from, callback.inline_message_id, poll, optionID);
                }
                await pollRepository.SaveAsync(poll);
            }
            finally
            {
                sem.Release();
            }
            await Refresh(callback.inline_message_id, pollID);
        }

        private async Task Refresh(string inlineMessageID, string pollID)
        {
            var poll = await pollRepository.LoadAsync(pollID);
            if (null != poll)
            {
                await Refresh(inlineMessageID, poll);
            }
        }

        private async Task Refresh(string inlineMessageID, Poll poll)
        {
            await api.EditInlineMessageTextAsync(inlineMessageID, poll.ConstructMessageText(messageService), "HTML", true, poll.ConstructVotingKeyboard(messageService));
        }

        private async Task ProcessVoteActionWhileLocked(TelegramApiCallbackQuery callback, TelegramApiUser user, string inlineMessageID, Poll poll, string optionID)
        {
            await Task.FromResult(0);

            if (poll.InlineMessageIDs == null) poll.InlineMessageIDs = new List<string>();
            if (!poll.InlineMessageIDs.Any(x => x == inlineMessageID))
            {
                poll.InlineMessageIDs.Add(inlineMessageID);
            }

            if (poll.Type == Poll.PollType.Single && null != poll.VoteOptions)
            {
                poll.VoteOptions
                    .Where(x => x.ID != optionID)
                    .Select(x => x.Votes)
                    .ToList()
                    .ForEach(votes => votes?.RemoveAll(y => y.id == user.id));
            }

            Poll.VoteOption option = poll.VoteOptions?.FirstOrDefault(x => x.ID == optionID);
            if (null != option)
            {
                if (null == option.Votes) option.Votes = new List<TelegramApiUser>();
                if (option.Votes.Any(x => x.id == user.id))
                {
                    option.Votes.RemoveAll(x => x.id == user.id);
                }
                else
                {
                    option.Votes.Add(user);
                }
            }
        }
    }
}
