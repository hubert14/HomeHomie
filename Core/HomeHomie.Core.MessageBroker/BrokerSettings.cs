namespace HomeHomie.Core.MessageBroker
{
    internal class BrokerSettings : IBrokerSettings
    {
        public string? VirtualHost { get; init; }

        public string? HostName { get; init; }
        public int? Port { get; init; }
        
        public string? UserName { get; init; }
        public string? Password { get; init; }
    }
}
