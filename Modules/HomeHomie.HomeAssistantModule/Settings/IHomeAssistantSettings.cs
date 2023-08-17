namespace HomeHomie.TelegramModule.Settings
{
    public interface IHomeAssistantSettings
    {
        public string? Address { get; }
        public string? AccessToken { get; }
    }
}
