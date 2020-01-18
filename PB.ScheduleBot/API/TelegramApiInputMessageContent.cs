namespace PB.ScheduleBot.API
{
    public class TelegramApiInputMessageContent
    {
        public TelegramApiInputMessageContent()
        {
            parse_mode = "HTML";
            disable_web_page_preview = true;
        }

        public string message_text { get; set; }

        public string parse_mode { get; set; }

        public bool disable_web_page_preview { get; set; }
    }
}