namespace PB.ScheduleBot.API
{
    public class TelegramApiInlineQuery
    {
        public string id { get; set; }
        public TelegramApiUser from { get; set; }
        public string query { get; set; }
        public string offset { get; set; }
    }
}