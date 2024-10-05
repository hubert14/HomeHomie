namespace HomeHomie.Core.MessageBroker
{
    public interface IBrokerSettings
    {
        public string? HostName { get; }
        public int? Port { get; }

        public string? VirtualHost { get; }
        public string? UserName { get; }
        public string? Password { get; }

        public void ValidateSettings()
        {

        }
    }
}
