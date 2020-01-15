namespace PB.ScheduleBot.API
{
    public class TelegramApiCallbackQuery
    {
        public int id { get; set;}
        public TelegramApiUser from { get; set;}
        public TelegramApiMessage message { get; set;  }
        public string inline_message_id { get; set; }
        public string chat_instance { get; set;}
        public string data { get; set; }
    }
}