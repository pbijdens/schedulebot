using PB.ScheduleBot.API;
using PB.ScheduleBot.Services;
using PB.ScheduleBot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PB.ScheduleBot.Model
{
    public class Poll
    {
        public class VoteOption
        {
            public VoteOption()
            {
                ID = shortid.ShortId.Generate(true, false, 12);
            }

            public string ID { get; set; }
            public string Text { get; set; }
            public List<TelegramApiUser> Votes { get; set; }
        }

        public enum PollType
        {
            Single,
            Multiple,
        }

        public string ID { get; set; }
        public string Subject { get; set; }
        public PollType Type { get; set; }
        public List<VoteOption> VoteOptions { get; set; }
        public bool IsClosed { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset ModificationDate { get; set; }
        public List<string> InlineMessageIDs { get; set; }

        public Poll()
        {
            ID = shortid.ShortId.Generate(true, false, 12);
            Subject = "";
            Type = PollType.Multiple;
            VoteOptions = new List<VoteOption>();
        }

        internal string AsListEntry()
        {
            string result = Subject;
            if (result.Length > 20)
            {
                result = result.Substring(0, 20);
            }
            if (IsClosed)
            {
                result = $"🚫 {result}";
            }
            return result;
        }

        public String ConstructMessageText(IMessageService messages)
        {
            StringBuilder messageBuilder = new StringBuilder();

            string subject = Subject;
            if (string.IsNullOrWhiteSpace(subject)) subject = messages.PollMessageNoSubjectText().HtmlSafe();
            messageBuilder.AppendLine($"<b>{subject.HtmlSafe()}</b>");
            messageBuilder.AppendLine($"<i>{PollTypeAsString(messages, Type)}</i>");
            if (IsClosed)
            {
                messageBuilder.AppendLine($"<b>{messages.PollMessagePollIsClosed().HtmlSafe()}</b>");
            }
            messageBuilder.AppendLine($"");
            if (null == VoteOptions || VoteOptions.Count == 0)
            {
                messageBuilder.AppendLine($"{messages.PollMessagePollHasNoVotingOptions().HtmlSafe()}");
            }
            else
            {
                foreach (var voteOption in VoteOptions ?? new List<VoteOption>())
                {
                    messageBuilder.AppendLine($"<b>{voteOption.Text.HtmlSafe()} ({voteOption.Votes?.Count ?? 0})</b>");
                    if (null != voteOption.Votes && voteOption.Votes.Count > 0)
                    {
                        foreach (var vote in voteOption.Votes)
                        {
                            messageBuilder.AppendLine($" - {ShortNameSafe(vote)}");
                        }
                        messageBuilder.AppendLine($"");
                    }
                    else
                    {
                        messageBuilder.AppendLine($"");
                    }
                }
            }
            messageBuilder.AppendLine($"<i>{DateTime.UtcNow.Ticks}</i>");
            return messageBuilder.ToString();
        }

        public TelegramApiInlineKeyboardMarkup ConstructInlineKeyboard(IMessageService messages)
        {
            List<TelegramApiInlineKeyboardButton[]> buttonRows = new List<TelegramApiInlineKeyboardButton[]>();

            TelegramApiInlineKeyboardMarkup markup = null;
            if (!IsDeleted)
            {
                if (IsClosed)
                {
                    buttonRows.Add(new TelegramApiInlineKeyboardButton[] {
                    new TelegramApiInlineKeyboardButton {
                            callback_data = $"edit.{ID}.reopen",
                            text = $"{messages.ButtonReopen()}",
                        }
                    });
                }
                else
                {
                    buttonRows.AddRange(new List<TelegramApiInlineKeyboardButton[]> {
                        new TelegramApiInlineKeyboardButton[] {
                            new TelegramApiInlineKeyboardButton {
                                callback_data = $"edit.{ID}.subject",
                                text = $"{messages.ButtonEditSubject()}",
                            },
                            new TelegramApiInlineKeyboardButton {
                                callback_data = $"edit.{ID}.type",
                                text = $"{messages.ButtonEditType()}",
                            },
                        }
                    });

                    if (VoteOptions != null && VoteOptions.Count >= 1)
                    {
                        buttonRows.Add(new TelegramApiInlineKeyboardButton[] {
                            new TelegramApiInlineKeyboardButton {
                                callback_data = $"edit.{ID}.add-voting-option",
                                text = $"{messages.ButtonAddOption()}",
                            },
                            new TelegramApiInlineKeyboardButton {
                                callback_data = $"edit.{ID}.remove-voting-option",
                                text = $"{messages.ButtonDeleteOption()}",
                            },
                            new TelegramApiInlineKeyboardButton {
                                callback_data = $"edit.{ID}.rename-voting-option",
                                text = $"{messages.ButtonEditOption()}",
                            },
                        });
                        buttonRows.Add(new TelegramApiInlineKeyboardButton[] {
                            new TelegramApiInlineKeyboardButton {
                                switch_inline_query = $"{ID}",
                                text = $"{messages.ButtonShare()}",
                            },
                     });
                    }
                    else
                    {
                        buttonRows.Add(new TelegramApiInlineKeyboardButton[] {
                            new TelegramApiInlineKeyboardButton {
                                callback_data = $"edit.{ID}.add-voting-option",
                                text = $"{messages.ButtonAddOption()}",
                            },
                        });
                    }

                    buttonRows.Add(new TelegramApiInlineKeyboardButton[] {
                        new TelegramApiInlineKeyboardButton {
                            callback_data = $"edit.{ID}.close",
                            text = $"{messages.ButtonClose()}",
                        },
                    });
                }
                buttonRows.Add(new TelegramApiInlineKeyboardButton[] {
                    new TelegramApiInlineKeyboardButton
                    {
                        callback_data = $"edit.{ID}.delete",
                        text = $"{messages.ButtonDelete()}",
                    },
                    new TelegramApiInlineKeyboardButton
                    {
                        callback_data = $"edit.{ID}.clone",
                        text = $"{messages.ButtonClone()}",
                    },
                });
            }
            buttonRows.Add(new TelegramApiInlineKeyboardButton[] {
                new TelegramApiInlineKeyboardButton {
                        callback_data = $"list",
                        text = $"{messages.ButtonBackToList()}",
                    }
                });

            markup = new TelegramApiInlineKeyboardMarkup
            {
                inline_keyboard = buttonRows.ToArray()
            };

            return markup;
        }

        public void AddButton(List<List<TelegramApiInlineKeyboardButton>> buttonRows, TelegramApiInlineKeyboardButton button)
        {
            if (buttonRows.Count == 0)
            {
                buttonRows.Add(new List<TelegramApiInlineKeyboardButton>());
            }
            var row = buttonRows.Last();
            int lastRowTotalLength = row.Sum(x => x.text.Length) + button.text.Length;
            int maxSingleButtonLength = row.Any() ? Math.Max(row.Max(x => x.text.Length), button.text.Length) : button.text.Length;

            if ((maxSingleButtonLength > 12 && row.Count >= 2)   // at least one button is too long for having three columns, and we already have two
                || (maxSingleButtonLength > 7 && row.Count >= 3) // at least one button is too long for having four columns, and we already have three
                || (row.Count >= 4) // there are 4 columns already
                || (lastRowTotalLength >= 20 && row.Count > 0) // Th elast row would be too wide, and it already has buttons on it
                )
            {
                buttonRows.Add(row = new List<TelegramApiInlineKeyboardButton>());
            }
            row.Add(button);
        }

        public TelegramApiInlineKeyboardMarkup ConstructVotingKeyboard(IMessageService messages)
        {
            List<List<TelegramApiInlineKeyboardButton>> buttonRows = new List<List<TelegramApiInlineKeyboardButton>>();

            if (!IsDeleted && !IsClosed && null != VoteOptions && VoteOptions.Count >= 1)
            {
                foreach (var option in VoteOptions)
                {
                    AddButton(buttonRows, new TelegramApiInlineKeyboardButton
                    {
                        callback_data = $"vote.{ID}.opt.{option.ID}",
                        text = $"{option.Text}"
                    });
                }
            }
            AddButton(buttonRows, new TelegramApiInlineKeyboardButton
            {
                callback_data = $"refresh.{ID}",
                text = $"{messages.ButtonRefresh()}"
            });

            TelegramApiInlineKeyboardMarkup markup = new TelegramApiInlineKeyboardMarkup
            {
                inline_keyboard = buttonRows.Select(x => x.ToArray()).ToArray()
            };

            return markup;
        }

        private string ShortNameSafe(TelegramApiUser user)
        {
            if (!string.IsNullOrWhiteSpace(user.username))
            {
                return user.username?.HtmlSafe();
            }
            else
            {
                return $"{user.first_name} {user.last_name}".HtmlSafe();
            }
        }

        private string PollTypeAsString(IMessageService messages, PollType type)
        {
            switch (type)
            {
                case PollType.Multiple:
                    return messages.PollMessageSelectMultiple().HtmlSafe();
                case PollType.Single:
                default:
                    return messages.PollMessageSelectOne().HtmlSafe();
            }
        }
    }
}
