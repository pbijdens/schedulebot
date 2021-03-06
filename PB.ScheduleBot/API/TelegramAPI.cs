using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PB.ScheduleBot.Services;

namespace PB.ScheduleBot.API
{
    public class TelegramAPI : ITelegramAPI
    {
        private static readonly HttpClient _client = new HttpClient();
        private readonly ILogger _logger;
        private readonly IBotConfiguration _botConfiguration;
        public string _apiURL = "";

        public TelegramAPI(ILogger logger, IBotConfiguration botConfiguration)
        {
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _logger = logger;
            _botConfiguration = botConfiguration;

            _apiURL = $"https://api.telegram.org/bot{_botConfiguration.Token}/";
        }

        public async Task SetWebHookAsync(string url)
        {
            await PostAsync($"{_apiURL}setWebHook", new { url });
        }

        public async Task<TelegramApiUser> GetMeAsync()
        {
            string postResult = await PostAsync($"{_apiURL}getMe", new { });
            TelegramApiUser result = JsonConvert.DeserializeObject<TelegramApiUser>(postResult);
            return result;
        }

        public async Task<TelegramApiMessage> SendMessageAsync(string chat_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, bool? disable_notification = null, int? reply_to_message_id = null, object reply_markup = null)
        {
            string postResult = await PostAsync($"{_apiURL}sendMessage", new { chat_id, text, parse_mode, disable_web_page_preview, disable_notification, reply_to_message_id, reply_markup });
            var result = JsonConvert.DeserializeObject<TelegramApiResult<TelegramApiMessage>>(postResult);
            return result?.result;
        }

        public async Task<TelegramApiMessage> SendMessageAsync(long chat_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, bool? disable_notification = null, int? reply_to_message_id = null, object reply_markup = null)
        {
            return await SendMessageAsync($"{chat_id}", text, parse_mode, disable_web_page_preview, disable_notification, reply_to_message_id, reply_markup);
        }

        public async Task<TelegramApiMessage> EditMessageTextAsync(string chat_id, string message_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, object reply_markup = null)
        {
            string postResult = await PostAsync($"{_apiURL}editMessageText", new { chat_id, message_id, text, parse_mode, disable_web_page_preview, reply_markup });
            var result = JsonConvert.DeserializeObject<TelegramApiResult<TelegramApiMessage>>(postResult);
            return result?.result;
        }

        public async Task<TelegramApiMessage> EditMessageTextAsync(long chat_id, string message_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, object reply_markup = null)
        {
            return await EditMessageTextAsync($"{chat_id}", message_id, text, parse_mode, disable_web_page_preview, reply_markup);
        }
        
        public async Task EditInlineMessageTextAsync(string inline_message_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, object reply_markup = null)
        {
            await PostAsync($"{_apiURL}editMessageText", new { inline_message_id, text, parse_mode, disable_web_page_preview, reply_markup });
        }

        public async Task AnswerInlineQueryAsync(string inline_query_id, TelegramApiInlineQueryResult[] results, int? cache_time = null, bool? is_personal = null, string next_offset = null, string switch_pm_text = null, string switch_pm_parameter = null)
        {
            await PostAsync($"{_apiURL}answerInlineQuery", new { inline_query_id, results, cache_time, is_personal, next_offset, switch_pm_text, switch_pm_parameter });
        }

        public async Task AnswerCallbackQuery(string callback_query_id, string text = null, bool? show_alert = null, string url = null, int? cache_time = null)
        {
            await PostAsync($"{_apiURL}answerCallbackQuery", new { callback_query_id, text, show_alert, url, cache_time });
        }

        public async Task DeleteMessageForChatAsync(long chat_id, long message_id)
        {
            try
            {
                await PostAsync($"{_apiURL}deleteMessage", new { chat_id, message_id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ffailed to delete message with ID {message_id} from chat {chat_id}");
            }
        }

        public async Task<string> PostAsync(string url, object body)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;

            string serializedBody = JsonConvert.SerializeObject(body, settings);
            var content = new StringContent(serializedBody, Encoding.UTF8, "application/json");

            _logger.LogInformation($"POST {url} WITH BODY {serializedBody}");

            var response = _client.PostAsync(url, content).Result;
            var responseBody = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"POST {url} RESULTED IN CODE {response.StatusCode} AND BODY {responseBody}");
            return responseBody;
        }
    }
}