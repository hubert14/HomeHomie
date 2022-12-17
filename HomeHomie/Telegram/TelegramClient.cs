using HomeHomie.Database;
using HomeHomie.Database.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HomeHomie.Telegram
{
    internal class TelegramClient
    {
        private TelegramBotClient _client;
        private CancellationTokenSource _cts;

        private static List<(long ChatId, string? Command)> StoredCommands = new();

        public TelegramClient()
        {
            var token = Environment.GetEnvironmentVariable(Variables.TELEGRAM_API_KEY);
            if (token == null)
            {
                Console.WriteLine("Telegram API key is not provided. Telegram bot will not work.");
            }

            _cts = new CancellationTokenSource();

            _client = new TelegramBotClient(token);
            _client.StartReceiving(HandleUpdateAsync, HandleErrorAsync, cancellationToken: _cts.Token);

            Console.WriteLine("Telegram Bot start's receiving messages");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
        }

        private async Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine("Error handled on Telegram bot side. Message:");
            Console.WriteLine(exception.Message);
        }

        private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    await ProcessMessageAsync(bot, update.Message!);
                    break;
                case UpdateType.Unknown:
                default:
                    break;
            }
        }

        private async Task ProcessMessageAsync(ITelegramBotClient bot, Message message)
        {
            var sender = message.From.Id;
            var availableChats = Environment.GetEnvironmentVariable(Variables.TELEGRAM_CHATS).Split('|');
            if (!availableChats.Contains(sender.ToString()))
            {
                Console.WriteLine("Message from not available user.");
                Console.WriteLine($"UserId: {sender.ToString()} | UserName: {message.From.Username}");
                Console.WriteLine($"Message: {message.Text}");
                return;
            }

            switch (message.Text)
            {
                case "/graphic":
                    await ProcessGraphicMessageAsync(bot, message);
                    break;
                case "/newgraphic":
                    await ProcessNewGraphicMessageAsync(bot, message);
                    break;
                case "/heater":
                    break;
                case "/kettle":
                    break;
                case "/sensors":
                    break;
                default:
                    await ProcessSimpleTextMessageAsync(bot, message);
                    break;
            }
        }

        private async Task ProcessGraphicMessageAsync(ITelegramBotClient bot, Message message)
        {
            var chatId = new ChatId(message.From.Id);
            var graphicForToday = await MongoProvider.GetDataFromMongoAsync();

            if (graphicForToday != null)
            {
                await bot.SendTextMessageAsync(chatId, graphicForToday.ToString(), ParseMode.Html);
            }
            else
            {
                await bot.SendTextMessageAsync(chatId, "К сожалению, на сегодня графика ещё нет");
            }
        }

        private async Task ProcessNewGraphicMessageAsync(ITelegramBotClient bot, Message message)
        {
            StoredCommands.Add((message.From.Id, "/newgraphic"));
            await bot.SendTextMessageAsync(new ChatId(message.From.Id), "Введите данные нового графика");
        }

        private async Task ProcessSimpleTextMessageAsync(ITelegramBotClient bot, Message message)
        {
            var storedCommand = StoredCommands.FirstOrDefault(x => x.ChatId == message.From!.Id);
            if (storedCommand.Command != null)
            {
                switch (storedCommand.Command)
                {
                    case "/newgraphic":
                        await ProcessSaveGraphicMessageAsync(bot, message);
                        StoredCommands.Remove(storedCommand);
                        break;
                }
            }
        }

        private async Task ProcessSaveGraphicMessageAsync(ITelegramBotClient bot, Message message)
        {
            var parsedGraphic = ElectricityGraphic.Parse(message.Text!);
            await MongoProvider.AddDataInMongoAsync(parsedGraphic);
            await bot.SendTextMessageAsync(new ChatId(message.From!.Id), "Записал, спасибо!");
        }
    }
}
