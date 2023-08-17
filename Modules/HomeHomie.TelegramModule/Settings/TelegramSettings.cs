namespace HomeHomie.TelegramModule.Settings
{
    internal class TelegramSettings : ITelegramSettings
    {
        public string? ApiKey { get; init; }
        public string[]? ChatsIds { get; init; }
        public string? ServiceChatId { get; init; }
    }
}
