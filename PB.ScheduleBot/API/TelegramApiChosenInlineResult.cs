namespace PB.ScheduleBot.API
{
    public class TelegramApiChosenInlineResult
    {
        public string result_id { get; set; }
        public TelegramApiUser from { get; set; }
        public string inline_message_id { get; set; }
        public string query { get; set; }
    }
}