namespace PB.ScheduleBot.API
{
    public class TelegramApiMessageEntity
    {
        public string type { get; set; }
        public int offset { get; set; }
        public int length { get; set; }
        public string url { get; set; }
        public TelegramApiUser user { get; set; }
    }
}