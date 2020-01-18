using System.Threading.Tasks;

namespace PB.ScheduleBot.API {
    public interface ITelegramAPI
    {
        Task<TelegramApiMessage> EditInlineMessageTextAsync(string inline_message_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, object reply_markup = null);
        Task<TelegramApiMessage> EditMessageTextAsync(string chat_id, string message_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, object reply_markup = null);
        Task<TelegramApiMessage> EditMessageTextAsync(long chat_id, string message_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, object reply_markup = null);
        Task<TelegramApiUser> GetMeAsync();
        Task<string> PostAsync(string url, object body);
        Task<TelegramApiMessage> SendMessageAsync(string chat_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, bool? disable_notification = null, int? reply_to_message_id = null, object reply_markup = null);
        Task<TelegramApiMessage> SendMessageAsync(long chat_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, bool? disable_notification = null, int? reply_to_message_id = null, object reply_markup = null);
        Task SetWebHookAsync(string url);
        Task AnswerInlineQueryAsync(string inline_query_id, TelegramApiInlineQueryResult[] results, int? cache_time = null, bool? is_personal = null, string next_offset = null, string switch_pm_text = null, string switch_pm_parameter = null);
        Task AnswerCallbackQuery(string callback_query_id, string text = null, bool? show_alert = null, string url = null, int? cache_time = null);
    }
}