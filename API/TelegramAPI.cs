using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace PB.ScheduleBot.API
{
    public class TelegramAPI
    {
        private static readonly HttpClient _client = new HttpClient();
        private readonly ILogger _logger;
        public string _apiURL = "";

        public TelegramAPI(ILogger logger, string token)
        {
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _apiURL = $"https://api.telegram.org/bot{token}/";
            _logger = logger;
        }

        public async void SetWebHook(string url)
        {
            await Post($"{_apiURL}setWebHook", new { url });
        }

        public async Task<TelegramApiUser> GetMe()
        {
            string postResult = await Post($"{_apiURL}getMe", new { });
            TelegramApiUser result = JsonConvert.DeserializeObject<TelegramApiUser>(postResult);
            return result;
        }

        public async Task<TelegramApiMessage> SendMessage(string chat_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, bool? disable_notification = null, int? reply_to_message_id = null, object reply_markup = null)
        {
            string postResult = await Post($"{_apiURL}sendMessage", new { chat_id, text, parse_mode, disable_web_page_preview, disable_notification, reply_to_message_id, reply_markup });
            TelegramApiMessage result = JsonConvert.DeserializeObject<TelegramApiMessage>(postResult);
            return result;
        }

        public async Task<TelegramApiMessage> SendMessage(int chat_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, bool? disable_notification = null, int? reply_to_message_id = null, object reply_markup = null)
        {
            return await SendMessage($"{chat_id}", text, parse_mode, disable_web_page_preview, disable_notification, reply_to_message_id, reply_markup);
        }

        public async Task<TelegramApiMessage> EditMessageText(string chat_id, string message_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, object reply_markup = null)
        {
            string postResult = await Post($"{_apiURL}editMessageText", new { chat_id, message_id, text, parse_mode, disable_web_page_preview, reply_markup });
            TelegramApiMessage result = JsonConvert.DeserializeObject<TelegramApiMessage>(postResult);
            return result;
        }

        public async Task<TelegramApiMessage> EditMessageText(int chat_id, string message_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, object reply_markup = null)
        {
            return await EditMessageText(chat_id, message_id, text, parse_mode, disable_web_page_preview, reply_markup);
        }
        
        public async Task<TelegramApiMessage> EditInlineMessageText(string inline_message_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, object reply_markup = null)
        {
            string postResult = await Post($"{_apiURL}editMessageText", new { inline_message_id, text, parse_mode, disable_web_page_preview, reply_markup });
            TelegramApiMessage result = JsonConvert.DeserializeObject<TelegramApiMessage>(postResult);
            return result;
        }

        public async Task<string> Post(string url, object body)
        {
            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            _logger.LogInformation($"POST {url} WITH BODY {body}");

            var response = _client.PostAsync(url, content).Result;
            var responseBody = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"POST {url} RESULTED IN CODE {response.StatusCode} AND BODY {responseBody}");
            return responseBody;
        }
    }
}