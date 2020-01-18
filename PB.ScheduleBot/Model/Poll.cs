using PB.ScheduleBot.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace PB.ScheduleBot.Model
{
    public class Poll
    {
        public class VoteOption
        {
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

        public Poll()
        {
            ID = shortid.ShortId.Generate(true, false, 12);
            Subject = "";
            Type = Poll.PollType.Multiple;
            VoteOptions = new List<Poll.VoteOption>();
        }
    }
}
