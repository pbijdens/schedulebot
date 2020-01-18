namespace PB.ScheduleBot.API
{
    public class TelegramApiMessage
    {
        public long message_id { get; set; }
        public TelegramApiUser from { get; set;}
        public long date { get; set; }
        public TelegramApiChat chat { get; set;}
        public TelegramApiMessage reply_to_message;
        public long edit_date { get; set; }
        public string text{ get; set;}
        public TelegramApiMessageEntity[] entities { get; set;}
    }
}