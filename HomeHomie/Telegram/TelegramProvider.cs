using System.Net.Http.Json;

namespace HomeHomie.Telegram
{
    static class TelegramProvider
    {
        static string apiKey = Environment.GetEnvironmentVariable("TELEGRAM_API_KEY");
        static string[] chats = Environment.GetEnvironmentVariable("TELEGRAM_CHATS").Split("|");

        public static async Task SendMessagesToTelegramChatsAsync(int offHour, int nearestOnHour)
        {
            var client = new HttpClient();

            foreach (var chat in chats)
            {
                var currentHour = DateTime.Now.Hour;

                var silent = currentHour > 7 ? false : true;

                var text = $"😵 Совсем скоро выключат свет!\nВремя отключения: {offHour}:00";
                if (nearestOnHour != 0) text += "\nСледующее включение: {nearestOnHour}:00";

                Console.WriteLine($"Start sending message to chat: {chat} | Silent: {silent} | Nearest On hour: {nearestOnHour}");

                string url = $"https://api.telegram.org/bot{apiKey}/sendMessage";
                var body = new
                {
                    chat_id = chat,
                    text,
                    disable_notification = silent
                };

                await client.PostAsJsonAsync(url, body);

                Console.WriteLine("Message sended to chat: " + chat);
            }
        }
    }
}
