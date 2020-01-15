namespace PB.ScheduleBot.API
{
    public class TelegramApiMessage
    {
        public int message_id { get; set; }
        public TelegramApiUser from { get; set;}
        public int date { get; set; }
        public TelegramApiChat chat { get; set;}
        public TelegramApiMessage reply_to_message;
        public int edit_date { get; set; }
        public string text{ get; set;}
    }
}