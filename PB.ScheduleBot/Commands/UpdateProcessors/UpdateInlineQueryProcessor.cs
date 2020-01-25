using PB.ScheduleBot.API;
using PB.ScheduleBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.ScheduleBot.Commands.UpdateProcessors
{
    public class UpdateInlineQueryProcessor : IUpdateInlineQueryProcessor
    {
        private readonly ITelegramAPI api;
        private readonly IPollRepository pollRepository;
        private readonly IUserStateRepository userStateRepository;
        private readonly ILogger logger;
        private readonly IMessageService messageService;

        public UpdateInlineQueryProcessor(ITelegramAPI api,
            IPollRepository pollRepository,
            IUserStateRepository userStateRepository,
            ILogger logger,
            IMessageService messageService)
        {
            this.api = api;
            this.pollRepository = pollRepository;
            this.userStateRepository = userStateRepository;
            this.logger = logger;
            this.messageService = messageService;
        }

        public async Task RunAsync(TelegramApiInlineQuery inlineQuery)
        {
            List<TelegramApiInlineQueryResult> results = new List<TelegramApiInlineQueryResult>();
            if (!string.IsNullOrWhiteSpace(inlineQuery.query))
            {
                logger.LogInformation($"Processing inline query for single item '{inlineQuery.query}'");
                var poll = await pollRepository.LoadAsync(inlineQuery.query);
                if (null != poll)
                {
                    logger.LogInformation($"Returning single result '{poll.Subject}'");
                    AddPollToInlineQuery(results, poll);
                }
                else
                {
                    logger.LogError($"There is no poll with ID '{inlineQuery.query}'");
                }
            }
            else
            {
                logger.LogInformation($"Processing inline query for all polls");
                var state = await userStateRepository.GetStateAsync(inlineQuery.from);
                if (null != state && state.OwnedPolls != null)
                {
                    var polls = new List<string>(state.OwnedPolls.ToArray());
                    polls.Reverse();
                    foreach (var pollID in polls.Take(10)) // 10 most recent, because of reverse :-)
                    {
                        var poll = await pollRepository.LoadAsync(pollID);
                        if (null != poll && !poll.IsClosed && !poll.IsDeleted)
                        {
                            logger.LogInformation($"Adding '{poll.Subject}' ({poll.ID})");
                            AddPollToInlineQuery(results, poll);
                        }
                    }
                }
            }

            await api.AnswerInlineQueryAsync(inlineQuery.id, results.ToArray(), 30, is_personal: true);
        }

        private void AddPollToInlineQuery(List<TelegramApiInlineQueryResult> results, Model.Poll poll)
        {
            results.Add(new TelegramApiInlineQueryResult
            {
                id = poll.ID,
                description = $"{poll.Subject} ({poll.Type})",
                input_message_content = new TelegramApiInputMessageContent
                {
                    disable_web_page_preview = true,
                    parse_mode = "HTML",
                    message_text = poll.ConstructMessageText(messageService).ToString()
                },
                reply_markup = poll.ConstructVotingKeyboard(messageService),
                title = poll.Subject,
            });
        }
    }
}
