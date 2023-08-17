using HomeHomie.Core.Providers;
using HomeHomie.TelegramModule.Models;
using HomeHomie.TelegramModule.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static HomeHomie.Core.Models.Messages.ElectricityGraphicMessages;
using static HomeHomie.Core.Models.Messages.TelegramMessages;

namespace HomeHomie.TelegramModule
{
    internal class TelegramReciever : INotificationConsumerProvider
    {
        private readonly TelegramBotClient _client;
        private readonly CancellationTokenSource _cts;
        private readonly ICacheProvider _cache;
        private readonly IBrokerProvider _messageBroker;
        private readonly string[] _chatsList;

        private bool _disposedValue;

        public TelegramReciever(ITelegramSettings settings, ICacheProvider cache, IBrokerProvider messageBroker)
        {
            _cts = new CancellationTokenSource();
            _client = new TelegramBotClient(settings.ApiKey ?? throw new ArgumentException("Telegram API KEY is not defined under Telegram:ApiKey setting"));
            _cache = cache;
            _messageBroker = messageBroker;
            _chatsList = settings.ChatsIds ?? throw new ArgumentException("Telegram Chats is not defined under Telegram:ChatsIds setting");
        }

        public void StartReceiving()
        {
            _client.StartReceiving(HandleUpdateAsync, HandleErrorAsync, cancellationToken: _cts.Token);
            Console.WriteLine("Telegram Bot start's receiving messages");
        }

        private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine("Error handled on Telegram bot side. Message:");
            Console.WriteLine(exception.Message);
            Console.WriteLine(exception.StackTrace);
            if (exception.InnerException is not null) Console.WriteLine(exception.InnerException.Message);

            return Task.CompletedTask;
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
            var sender = message.From!.Id.ToString();
            if (!_chatsList.Contains(sender))
            {
                Console.WriteLine("Message from not available user.");
                Console.WriteLine($"UserId: {sender} | UserName: {message.From.Username}");
                Console.WriteLine($"Message: {message.Text}");
                return;
            }

            Console.WriteLine($"Got message from {sender}");
            Console.WriteLine("Message:");
            Console.WriteLine(message.Text);

            switch (message.Text)
            {
                case "/graphic":
                    await ProcessGraphicMessageAsync(message);
                    break;
                case "/newgraphic":
                    await ProcessNewGraphicMessageAsync(message);
                    break;
                case "/heater":
                case "/kettle":
                case "/sensors":
                    break;
                default:
                    await ProcessSimpleTextMessageAsync(message);
                    break;
            }
        }

        private Task ProcessGraphicMessageAsync(Message message)
        {
            _messageBroker.SendMessage(new SendGraphicRequestMessage { From = message.From!.Id.ToString() });
            return Task.CompletedTask;
        }

        private async Task ProcessNewGraphicMessageAsync(Message message)
        {
            const string message_template = "Введите данные нового графика";
            var chatId = message.From!.Id.ToString();
            await AddCommandToCacheAsync(chatId, "/newgraphic");
            _messageBroker.SendMessage(new SendTelegramMessageRequest
            {
                Message = message_template,
                ChatId = chatId
            });
        }

        private async Task ProcessSimpleTextMessageAsync(Message message)
        {
            var chatId = message.From!.Id.ToString();
            var storedCommands = await GetCommandsFromCacheAsync(chatId);
            if (storedCommands?.Any() ?? false)
            {
                // TODO: Decide need to store only last command or all commands?
                switch (storedCommands.Last())
                {
                    case "/newgraphic":
                        _messageBroker.SendMessage(new SendNewGraphicRequestMessage { From = chatId, Body = message.Text! });
                        await RemoveCommandFromCacheAsync(chatId);
                        break;
                }
            }
        }

        //private async Task ProcessSaveGraphicMessageAsync(ITelegramBotClient bot, Message message)
        //{


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _cts.Cancel();
                _cts.Dispose();
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public Task NotifyMessageAsync(string text)
        {
            throw new NotImplementedException();
        }

        public Task NotifyMediaMessageAsync(string text, Stream media)
        {
            throw new NotImplementedException();
        }

        private async Task AddCommandToCacheAsync(string chatId, string command)
        {
            var key = StoredCommandCacheKey(chatId);
            var existed = await _cache.GetAsync<StoredCommandCacheModel>(key);
            if (existed == null)
            {
                await _cache.SetAsync(key, new List<string> { command });
            }
            else
            {
                existed.Commands.Add(command);
                await _cache.SetAsync(key, command);
            }
        }

        private async Task<List<string>?> GetCommandsFromCacheAsync(string chatId)
        {
            var result = await _cache.GetAsync<StoredCommandCacheModel>(StoredCommandCacheKey(chatId));
            return result?.Commands;
        }

        private async Task RemoveCommandFromCacheAsync(string chatId)
        {
            await _cache.RemoveAsync(StoredCommandCacheKey(chatId));
        }

        private string StoredCommandCacheKey(string chatId) => $"tg_{chatId}_sc";
    }
}
