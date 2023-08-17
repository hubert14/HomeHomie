namespace HomeHomie.TelegramModule.Settings
{
    internal class HomeAssistantSettings : IHomeAssistantSettings
    {
        public string? Address { get; init; }
        public string AccessToken { get; init; }
    }
}
