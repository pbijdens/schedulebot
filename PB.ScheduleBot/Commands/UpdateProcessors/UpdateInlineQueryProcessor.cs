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

        public UpdateInlineQueryProcessor(ITelegramAPI api,
            IPollRepository pollRepository,
            IUserStateRepository userStateRepository,
            ILogger logger)
        {
            this.api = api;
            this.pollRepository = pollRepository;
            this.userStateRepository = userStateRepository;
            this.logger = logger;
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
                    var polls = state.OwnedPolls;
                    polls.Reverse();
                    foreach (var pollID in polls.Take(10))
                    {
                        var poll = await pollRepository.LoadAsync(pollID);
                        if (null != poll && !poll.IsClosed)
                        {
                            logger.LogInformation($"Adding '{poll.Subject}' ({poll.ID})");
                            AddPollToInlineQuery(results, poll);
                        }
                    }
                }
            }

            await api.AnswerInlineQueryAsync(inlineQuery.id, results.ToArray(), 30, is_personal: true);
        }

        private static void AddPollToInlineQuery(List<TelegramApiInlineQueryResult> results, Model.Poll poll)
        {
            results.Add(new TelegramApiInlineQueryResult
            {
                id = poll.ID,
                description = $"{poll.Subject} ({poll.Type})",
                input_message_content = new TelegramApiInputMessageContent
                {
                    disable_web_page_preview = true,
                    parse_mode = "HTML",
                    message_text = "Dit is een test"
                },
                reply_markup = null,
                title = poll.Subject,
            });
        }
    }
}
