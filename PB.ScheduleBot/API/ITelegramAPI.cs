using System.Threading.Tasks;

namespace PB.ScheduleBot.API {
    public interface ITelegramAPI
    {
        Task<TelegramApiMessage> EditInlineMessageTextAsync(string inline_message_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, object reply_markup = null);
        Task<TelegramApiMessage> EditMessageTextAsync(string chat_id, string message_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, object reply_markup = null);
        Task<TelegramApiMessage> EditMessageTextAsync(int chat_id, string message_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, object reply_markup = null);
        Task<TelegramApiUser> GetMeAsync();
        Task<string> PostAsync(string url, object body);
        Task<TelegramApiMessage> SendMessageAsync(string chat_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, bool? disable_notification = null, int? reply_to_message_id = null, object reply_markup = null);
        Task<TelegramApiMessage> SendMessageAsync(int chat_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, bool? disable_notification = null, int? reply_to_message_id = null, object reply_markup = null);
        Task SetupAsync(string token);
        Task SetWebHookAsync(string url);
    }
}