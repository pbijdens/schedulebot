namespace PB.ScheduleBot.API
{
    public class TelegramApiUpdate
    {
        public int update_id { get; set; }
        public TelegramApiMessage message { get; set; }
        public TelegramApiMessage edited_message { get; set; }
        public TelegramApiMessage channel_post { get; set; }
        public TelegramApiMessage edited_channel_post { get; set; }
        public TelegramApiInlineQuery inline_query { get; set; }
        public TelegramApiChosenInlineResult chosen_inline_result { get; set; }
        public TelegramApiCallbackQuery callback_query { get; set; }
    }
}