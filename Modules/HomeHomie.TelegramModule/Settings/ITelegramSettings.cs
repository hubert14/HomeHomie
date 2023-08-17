namespace HomeHomie.TelegramModule.Settings
{
    public interface ITelegramSettings
    {
        public string? ApiKey { get; }
        public string[]? ChatsIds { get; }

        public string? ServiceChatId { get; }
    }
}
