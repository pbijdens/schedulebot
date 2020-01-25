using PB.ScheduleBot.API;
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

        public String ConstructMessageText()
        {
            StringBuilder messageBuilder = new StringBuilder();

            string subject = Subject;
            if (string.IsNullOrWhiteSpace(subject)) subject = "No subject was set for this this (yet)";
            messageBuilder.AppendLine($"<b>{subject.HtmlSafe()}</b>");
            messageBuilder.AppendLine($"<i>{PollTypeAsString(Type)}</i>");
            messageBuilder.AppendLine($"");
            if (null == VoteOptions || VoteOptions.Count == 0)
            {
                messageBuilder.AppendLine($"Vote options have not been set up for this poll yet.");
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
                        //messageBuilder.AppendLine($"<i>No votes yet.</i>");
                        messageBuilder.AppendLine($"");
                    }
                }
            }
            messageBuilder.AppendLine($"<i>{DateTime.UtcNow.Ticks}</i>");
            return messageBuilder.ToString();
        }

        public TelegramApiInlineKeyboardMarkup ConstructInlineKeyboard()
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
                            text = "↩️ Re-open this poll",
                        }
                    });
                }
                else
                {
                    buttonRows.AddRange(new List<TelegramApiInlineKeyboardButton[]> {
                        new TelegramApiInlineKeyboardButton[] {
                            new TelegramApiInlineKeyboardButton {
                                callback_data = $"edit.{ID}.subject",
                                text = "✏️ Subject",
                            },
                            new TelegramApiInlineKeyboardButton {
                                callback_data = $"edit.{ID}.type",
                                text = "✏️ Type",
                            },
                        }
                    });

                    if (VoteOptions != null && VoteOptions.Count >= 1)
                    {
                        buttonRows.Add(new TelegramApiInlineKeyboardButton[] {
                            new TelegramApiInlineKeyboardButton {
                                callback_data = $"edit.{ID}.add-voting-option",
                                text = "➕ Option",
                            },
                            new TelegramApiInlineKeyboardButton {
                                callback_data = $"edit.{ID}.remove-voting-option",
                                text = "➖ Option",
                            },
                            new TelegramApiInlineKeyboardButton {
                                callback_data = $"edit.{ID}.rename-voting-option",
                                text = "✏️ Option",
                            },
                        });
                        buttonRows.Add(new TelegramApiInlineKeyboardButton[] {
                            new TelegramApiInlineKeyboardButton {
                                switch_inline_query = $"{ID}",
                                text = "Share",
                            },
                     });
                    }
                    else
                    {
                        buttonRows.Add(new TelegramApiInlineKeyboardButton[] {
                            new TelegramApiInlineKeyboardButton {
                                callback_data = $"edit.{ID}.add-voting-option",
                                text = "➕ Option",
                            },
                        });
                    }

                    buttonRows.Add(new TelegramApiInlineKeyboardButton[] {
                        new TelegramApiInlineKeyboardButton {
                            callback_data = $"edit.{ID}.close",
                            text = "🚫 Close",
                        },
                    });
                }
                buttonRows.Add(new TelegramApiInlineKeyboardButton[] {
                    new TelegramApiInlineKeyboardButton
                    {
                        callback_data = $"edit.{ID}.delete",
                        text = "🗑 Delete",
                    },
                    new TelegramApiInlineKeyboardButton
                    {
                        callback_data = $"edit.{ID}.clone",
                        text = "♻️ Clone",
                    },
                });
            }
            buttonRows.Add(new TelegramApiInlineKeyboardButton[] {
                new TelegramApiInlineKeyboardButton {
                        callback_data = $"list",
                        text = "🔙 Back to the list",
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

        public TelegramApiInlineKeyboardMarkup ConstructVotingKeyboard()
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
                text = $"🔄 Refresh"
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

        private string PollTypeAsString(PollType type)
        {
            switch (type)
            {
                case PollType.Multiple:
                    return "Select one or more of the following options";
                case PollType.Single:
                default:
                    return "Select one of the following options";
            }
        }
    }
}
