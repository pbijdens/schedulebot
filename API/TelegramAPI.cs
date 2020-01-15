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
    public class TelegramAPI : ITelegramAPI
    {
        private static readonly HttpClient _client = new HttpClient();
        private readonly ILogger _logger;
        public string _apiURL = "";

        public TelegramAPI(ILogger logger)
        {
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _logger = logger;
        }

        public async Task SetupAsync(string token)
         {
            _apiURL = $"https://api.telegram.org/bot{token}/";
            await Task.FromResult(0);
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
            TelegramApiMessage result = JsonConvert.DeserializeObject<TelegramApiMessage>(postResult);
            return result;
        }

        public async Task<TelegramApiMessage> SendMessageAsync(int chat_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, bool? disable_notification = null, int? reply_to_message_id = null, object reply_markup = null)
        {
            return await SendMessageAsync($"{chat_id}", text, parse_mode, disable_web_page_preview, disable_notification, reply_to_message_id, reply_markup);
        }

        public async Task<TelegramApiMessage> EditMessageTextAsync(string chat_id, string message_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, object reply_markup = null)
        {
            string postResult = await PostAsync($"{_apiURL}editMessageText", new { chat_id, message_id, text, parse_mode, disable_web_page_preview, reply_markup });
            TelegramApiMessage result = JsonConvert.DeserializeObject<TelegramApiMessage>(postResult);
            return result;
        }

        public async Task<TelegramApiMessage> EditMessageTextAsync(int chat_id, string message_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, object reply_markup = null)
        {
            return await EditMessageTextAsync(chat_id, message_id, text, parse_mode, disable_web_page_preview, reply_markup);
        }
        
        public async Task<TelegramApiMessage> EditInlineMessageTextAsync(string inline_message_id, string text, string parse_mode = "HTML", bool? disable_web_page_preview = null, object reply_markup = null)
        {
            string postResult = await PostAsync($"{_apiURL}editMessageText", new { inline_message_id, text, parse_mode, disable_web_page_preview, reply_markup });
            TelegramApiMessage result = JsonConvert.DeserializeObject<TelegramApiMessage>(postResult);
            return result;
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