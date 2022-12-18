using System.Net.Http.Json;
using static HomeHomie.Utils;

namespace HomeHomie.Telegram
{
    static class TelegramProvider
    {
        static string apiKey = GetVariable("TELEGRAM_API_KEY");
        static string[] chats = GetVariable("TELEGRAM_CHATS").Split("|");

        public static async Task SendMessagesToTelegramChatsAsync(string text)
        {
            var client = new HttpClient();

            foreach (var chat in chats)
            {
                var currentHour = DateTime.Now.Hour;

                var silent = currentHour > 7 ? false : true;


                Console.WriteLine($"Start sending message to chat: {chat} | Silent: {silent}");
                Console.WriteLine($"Text: {text}");

                string url = $"https://api.telegram.org/bot{apiKey}/sendMessage";
                var body = new
                {
                    chat_id = chat,
                    text,
                    disable_notification = silent
                };

                var response = await client.PostAsJsonAsync(url, body);
                var json = await response.Content.ReadFromJsonAsync<dynamic>();
                if (json != null)
                {
                    Console.WriteLine($"Message sended to chat: {chat} | MessageId: {json.message_id}");
                }
            }
        }

        public static async Task SendMediaMessageAsync(string imageLink, string message)
        {
            var client = new HttpClient();

            var image = await client.GetStreamAsync(imageLink);

            foreach (var chat in chats)
            {
                var currentHour = DateTime.Now.Hour;

                var silent = currentHour > 7 ? false : true;

                Console.WriteLine($"Start sending media message to chat: {chat} | Silent: {silent}");
                Console.WriteLine($"Image: {imageLink}");
                Console.WriteLine($"Text: {message}");

                string url = $"https://api.telegram.org/bot{apiKey}/sendPhoto";
                var formContent = new MultipartFormDataContent
                {
                    {new StringContent(chat),"chat_id"},
                    {new StringContent(message),"caption"},
                    {new StringContent("html"),"parse_mode"},
                    {new StreamContent(image),"photo", "graphic"}
                };

                await client.PostAsync(url, formContent);
                Console.WriteLine($"Message sended to chat: {chat}");
            }
        }
    }
}
