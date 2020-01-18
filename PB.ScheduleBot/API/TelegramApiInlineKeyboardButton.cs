namespace PB.ScheduleBot.API
{
    public class TelegramApiInlineKeyboardButton
    {
        public string text { get; set; }
        public string url { get; set; }
        // Optional. Data to be sent in a callback query to the bot when button is pressed, 1-64 bytes
        public string callback_data { get; set; }
        // Optional. If set, pressing the button will prompt the user to select one of their chats, open that chat and insert the bot‘s username and the specified inline query in the input field. Can be empty, in which case just the bot’s username will be inserted. This offers an easy way for users to start using your bot in inline mode when they are currently in a private chat with it. Especially useful when combined with switch_pm… actions – in this case the user will be automatically returned to the chat they switched from, skipping the chat selection screen.
        public string switch_inline_query { get; set; }
    }
}