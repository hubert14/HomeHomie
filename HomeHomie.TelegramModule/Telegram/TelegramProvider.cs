using HomeHomie.Core.Constants;
using HomeHomie.Core.Providers;
using HomeHomie.TelegramModule.Models;
using System.Net.Http.Json;
using static HomeHomie.Core.Models.Messages.TelegramMessages;

namespace HomeHomie.TelegramModule.Telegram
{
    class TelegramResponse
    {
        public class TelegramResult
        {
            public int message_id { get; set; }
        }

        public TelegramResult? result { get; set; }
    }

    internal class TelegramProvider : INotificationProducerProvider
    {
        private readonly string _apiKey;
        private readonly IBrokerProvider _messageBroker;
        private readonly string[] _chatsList;
        
        private Guid _recieveKey;

        public TelegramProvider(ITelegramSettings settings, IBrokerProvider messageBroker)
        {
            _apiKey = settings.ApiKey ?? throw new ArgumentException("Telegram API KEY is not defined under Telegram:ApiKey setting");
            _chatsList = settings.ChatsIds ?? throw new ArgumentException("Telegram Chats is not defined under Telegram:ChatsIds setting");
            _messageBroker = messageBroker;
        }

        public void StartProducing()
        {
            _recieveKey = _messageBroker.StartRecieving<SendTelegramMessageRequest>(QueuesList.Telegram.SendTelegramMessageRequest, ProcessSendMessageRequest);
        }

        private async Task ProcessSendMessageRequest(SendTelegramMessageRequest? request)
        {
            if (request == null) return;
            if (!string.IsNullOrWhiteSpace(request.MediaLink))
            {
                await ProcessSendMediaMessageRequest(request);
                return;
            }

            string[]? chats;
            if (!string.IsNullOrWhiteSpace(request.ChatId)) chats = new[] { request.ChatId };
            else chats = _chatsList;

            var client = new HttpClient();

            foreach (var chat in chats)
            {
                Console.WriteLine($"Start sending message to chat: {chat} | Silent: {IsSilent}");
                Console.WriteLine($"Text: {request.Message}");

                string url = $"https://api.telegram.org/bot{_apiKey}/sendMessage";
                var body = new
                {
                    chat_id = chat,
                    text = request.Message,
                    disable_notification = IsSilent
                };

                bool isSuccess = false;
                var retryCount = 10;
                while (!isSuccess && --retryCount != 0)
                {
                    await Task.Delay(1_500);
                    var response = await client.PostAsJsonAsync(url, body);
                    isSuccess = response.IsSuccessStatusCode;

                    if (!isSuccess) continue;

                    var json = await response.Content.ReadFromJsonAsync<TelegramResponse>();
                    _messageBroker.SendMessage(new SendTelegramMessageResponse(chat, json!.result!.message_id.ToString()));
                }

                Console.WriteLine($"Message sended to chat: {chat}");
            }
        }

        private async Task ProcessSendMediaMessageRequest(SendTelegramMessageRequest request)
        {
            if (request == null) return;
            if (string.IsNullOrWhiteSpace(request.MediaLink))
            {
                throw new Exception("Message doesn't contains any media links");
            }

            var client = new HttpClient();
            var image = await client.GetStreamAsync(request.MediaLink);
            using var ms = new MemoryStream();
            image.CopyTo(ms);

            string[]? chats;
            if (!string.IsNullOrWhiteSpace(request.ChatId)) chats = new[] { request.ChatId };
            else chats = _chatsList;

            var messages = new List<TelegramMessage>();

            foreach (var chat in chats)
            {
                Console.WriteLine($"Start sending media message to chat: {chat} | Silent: {IsSilent}");
                Console.WriteLine($"Image: {request.MediaLink}");
                Console.WriteLine($"Text: {request}");

                string url = $"https://api.telegram.org/bot{_apiKey}/sendPhoto";
                ms.Seek(0, SeekOrigin.Begin);
                var formContent = new MultipartFormDataContent
                {
                    {new StringContent(chat.ToString()),"chat_id"},
                    {new StringContent(request.Message),"caption"},
                    {new StringContent("html"),"parse_mode"},
                    {new StringContent(IsSilent.ToString()),"disable_notifications"},
                    {new StreamContent(ms),"photo", "graphic"}
                };

                bool isSuccess = false;
                var retryCount = 10;
                while (!isSuccess && --retryCount != 0)
                {
                    await Task.Delay(1_500);
                    var response = await client.PostAsync(url, formContent);
                    isSuccess = response.IsSuccessStatusCode;
                    if (!isSuccess)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("ERROR: ");
                        Console.WriteLine(content);
                    }
                    else
                    {
                        var json = await response.Content.ReadFromJsonAsync<TelegramResponse>();
                        SendSuccessMessageToBroker(chat, json!.result!.message_id.ToString());
                        Console.WriteLine($"Message sended to chat: {chat}");
                    }
                }
            }
        }

        private void SendSuccessMessageToBroker(string chatId, string messageId)
            => _messageBroker.SendMessage(new SendTelegramMessageResponse(chatId, messageId));

        public void Dispose()
        {
            _messageBroker.StopRecieving(_recieveKey);
        }

        private static bool IsSilent => DateTime.Now.Hour <= 7;

    }
}
